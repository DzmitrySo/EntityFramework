// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Query.ResultOperators;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Caching.Memory;
using Remotion.Linq;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Microsoft.Data.Entity.Query
{
    public class CompiledQueryCache : ICompiledQueryCache
    {
        public const string CompiledQueryParameterPrefix = "__";

        private static readonly object _compiledQueryLockObject = new object();

        private class CompiledQuery
        {
            public Type ResultItemType;
            public Delegate Executor;
        }

        private readonly IMemoryCache _memoryCache;

        public CompiledQueryCache([NotNull] IMemoryCache memoryCache)
        {
            Check.NotNull(memoryCache, nameof(memoryCache));

            _memoryCache = memoryCache;
        }

        public virtual TResult Execute<TResult>(
            Expression query, IDataStore dataStore, QueryContext queryContext)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(dataStore, nameof(dataStore));
            Check.NotNull(queryContext, nameof(queryContext));

            var compiledQuery
                = GetOrAdd(query, queryContext, dataStore, isAsync: false, compiler: (q, ds) =>
                    {
                        var queryModel = CreateQueryParser().GetParsedQuery(q);

                        var streamedSequenceInfo
                            = queryModel.GetOutputDataInfo() as StreamedSequenceInfo;

                        var resultItemType
                            = streamedSequenceInfo?.ResultItemType ?? typeof(TResult);

                        var executor
                            = CompileQuery(ds, DataStore.CompileQueryMethod, resultItemType, queryModel);

                        return new CompiledQuery
                            {
                                ResultItemType = resultItemType,
                                Executor = executor
                            };
                    });

            return
                typeof(TResult) == compiledQuery.ResultItemType
                    ? ((Func<QueryContext, IEnumerable<TResult>>)compiledQuery.Executor)(queryContext).First()
                    : ((Func<QueryContext, TResult>)compiledQuery.Executor)(queryContext);
        }

        public virtual IAsyncEnumerable<TResult> ExecuteAsync<TResult>(
            Expression query, IDataStore dataStore, QueryContext queryContext)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(dataStore, nameof(dataStore));
            Check.NotNull(queryContext, nameof(queryContext));

            var compiledQuery
                = GetOrAdd(query, queryContext, dataStore, isAsync: true, compiler: (q, ds) =>
                    {
                        var queryModel = CreateQueryParser().GetParsedQuery(q);

                        var executor
                            = CompileQuery(ds, DataStore.CompileAsyncQueryMethod, typeof(TResult), queryModel);

                        return new CompiledQuery
                            {
                                ResultItemType = typeof(TResult),
                                Executor = executor
                            };
                    });

            return ((Func<QueryContext, IAsyncEnumerable<TResult>>)compiledQuery.Executor)(queryContext);
        }

        public virtual Task<TResult> ExecuteAsync<TResult>(
            Expression query, IDataStore dataStore, QueryContext queryContext, CancellationToken cancellationToken)
        {
            Check.NotNull(query, nameof(query));
            Check.NotNull(dataStore, nameof(dataStore));
            Check.NotNull(queryContext, nameof(queryContext));

            var compiledQuery
                = GetOrAdd(query, queryContext, dataStore, isAsync: true, compiler: (q, ds) =>
                    {
                        var queryModel = CreateQueryParser().GetParsedQuery(q);

                        var executor
                            = CompileQuery(ds, DataStore.CompileAsyncQueryMethod, typeof(TResult), queryModel);

                        return new CompiledQuery
                            {
                                ResultItemType = typeof(TResult),
                                Executor = executor
                            };
                    });

            return ((Func<QueryContext, IAsyncEnumerable<TResult>>)compiledQuery.Executor)(queryContext)
                .First(cancellationToken);
        }

        private CompiledQuery GetOrAdd(
            Expression query,
            QueryContext queryContext,
            IDataStore dataStore,
            bool isAsync,
            Func<Expression, IDataStore, CompiledQuery> compiler)
        {
            query = new QueryAnnotationWrappingExpressionTreeVisitor()
                .VisitExpression(query);

            var parameterizedQuery
                = ParameterExtractingExpressionTreeVisitor
                    .ExtractParameters(query, queryContext);

            var cacheKey
                = dataStore.Model.GetHashCode().ToString()
                  + isAsync
                  + new ExpressionStringBuilder()
                      .Build(query);

            CompiledQuery compiledQuery;
            lock (_compiledQueryLockObject)
            {
                if (!_memoryCache.TryGetValue(cacheKey, out compiledQuery))
                {
                    compiledQuery = compiler(parameterizedQuery, dataStore);
                    _memoryCache.Set(cacheKey, compiledQuery);
                }
            }

            return compiledQuery;
        }

        private static Delegate CompileQuery(
            IDataStore dataStore, MethodInfo compileMethodInfo, Type resultItemType, QueryModel queryModel)
        {
            try
            {
                return (Delegate)compileMethodInfo
                    .MakeGenericMethod(resultItemType)
                    .Invoke(dataStore, new object[] { queryModel });
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();

                throw;
            }
        }

        private static QueryParser CreateQueryParser()
            => new QueryParser(
                new ExpressionTreeParser(
                    CreateNodeTypeProvider(),
                    new CompoundExpressionTreeProcessor(new IExpressionTreeProcessor[]
                        {
                            new PartialEvaluatingExpressionTreeProcessor(),
                            new TransformingExpressionTreeProcessor(ExpressionTransformerRegistry.CreateDefault())
                        })));

        private static CompoundNodeTypeProvider CreateNodeTypeProvider()
        {
            var searchedTypes
                = typeof(MethodInfoBasedNodeTypeRegistry)
                    .GetTypeInfo()
                    .Assembly
                    .DefinedTypes
                    .Select(ti => ti.AsType())
                    .ToList();

            var methodInfoBasedNodeTypeRegistry
                = MethodInfoBasedNodeTypeRegistry.CreateFromTypes(searchedTypes);

            methodInfoBasedNodeTypeRegistry
                .Register(QueryAnnotationExpressionNode.SupportedMethods, typeof(QueryAnnotationExpressionNode));

            methodInfoBasedNodeTypeRegistry
                .Register(IncludeExpressionNode.SupportedMethods, typeof(IncludeExpressionNode));

            methodInfoBasedNodeTypeRegistry
                .Register(ThenIncludeExpressionNode.SupportedMethods, typeof(ThenIncludeExpressionNode));

            var innerProviders
                = new INodeTypeProvider[]
                    {
                        methodInfoBasedNodeTypeRegistry,
                        MethodNameBasedNodeTypeRegistry.CreateFromTypes(searchedTypes)
                    };

            return new CompoundNodeTypeProvider(innerProviders);
        }
    }
}
