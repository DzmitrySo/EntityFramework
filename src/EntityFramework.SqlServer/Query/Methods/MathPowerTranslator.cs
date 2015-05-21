// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.SqlServer.Query.Methods
{
    public class MathPowerTranslator : IMethodCallTranslator
    {
        public virtual Expression Translate([NotNull] MethodCallExpression methodCallExpression)
        {
            var methodInfo = typeof(Math).GetTypeInfo().GetDeclaredMethod("Pow");
            if (methodInfo == methodCallExpression.Method)
            {
                return new SqlFunctionExpression("POWER", methodCallExpression.Arguments, methodCallExpression.Type);
            }

            return null;
        }
    }
}
