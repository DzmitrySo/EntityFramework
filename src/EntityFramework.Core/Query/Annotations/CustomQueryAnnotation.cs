// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;

namespace Microsoft.Data.Entity.Query.Annotations
{
    public class CustomQueryAnnotation : QueryAnnotation
    {
        private readonly MethodInfo _methodInfo;
        private readonly object[] _arguments;

        public CustomQueryAnnotation(
            [NotNull] MethodInfo methodInfo,
            [NotNull] object[] arguments)
        {
            _methodInfo = Check.NotNull(methodInfo, nameof(methodInfo));
            _arguments = Check.NotNull(arguments, nameof(arguments));
        }

        public virtual MethodInfo MethodInfo => _methodInfo;

        public virtual IReadOnlyList<object> Arguments => _arguments;

        public virtual bool IsCallTo([NotNull] MethodInfo methodInfo)
        {
            Check.NotNull(methodInfo, nameof(methodInfo));

            return MethodInfo.GetGenericMethodDefinition().Equals(methodInfo);
        }

        public override string ToString()
            => "AnnotateQuery("
            + _methodInfo.Name
            + "("
            + Arguments.Select(FormatArgument).Join()
            + "))";

        private static string FormatArgument(object argument)
        {
            if (argument != null && argument.GetType().IsArray)
            {
                return "["
                       + ((IEnumerable)argument).Cast<object>().Select(FormatArgument).Join()
                       + "]";
            }

            return FormattingExpressionTreeVisitor.Format(Expression.Constant(argument));
        }
    }
}
