// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Framework.Logging;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteFunctionTranslationProvider : RelationalFunctionTranslationProvider
    {
        private List<IMethodCallTranslator> _sqliteMethodCallTranslators;
        private List<IPropertyTranslator> _sqlitePropertyTranslators;

        public SqliteFunctionTranslationProvider([NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            _sqliteMethodCallTranslators = new List<IMethodCallTranslator>();

            _sqlitePropertyTranslators = new List<IPropertyTranslator>();
        }

        public override IReadOnlyList<IMethodCallTranslator> MethodCallTranslators
            => base.MethodCallTranslators.Concat(_sqliteMethodCallTranslators).ToList();

        public override IReadOnlyList<IPropertyTranslator> PropertyTranslators
            => base.PropertyTranslators.Concat(_sqlitePropertyTranslators).ToList();
    }
}
