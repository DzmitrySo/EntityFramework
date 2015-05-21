// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using JetBrains.Annotations;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class SqlFunctionExpression : ExtensionExpression
    {
        public SqlFunctionExpression(
            [NotNull] string functionName,
            [NotNull] IEnumerable<Expression> arguments,
            [NotNull] Type returnType)
            : base(returnType)
        {
            FunctionName = functionName;
            Arguments = new ReadOnlyCollection<Expression>(arguments.ToList());
        }

        public virtual string FunctionName { get; [param: NotNull] set; }

        public virtual ReadOnlyCollection<Expression> Arguments { get; private set; }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitSqlFunctionExpression(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            var arguments = visitor.VisitAndConvert(Arguments, "VisitChildren");

            return arguments != Arguments
                ? new SqlFunctionExpression(FunctionName, arguments, Type)
                : this;
        }
    }
}
