// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;

namespace Microsoft.Data.Entity.Internal
{
    public static class QueryAnnotationExtensions
    {
        internal static readonly MethodInfo AnnotateQueryMethodInfo
            = typeof(QueryAnnotationExtensions)
                .GetTypeInfo().GetDeclaredMethod(nameof(AnnotateQuery));

        internal static IQueryable<TEntity> AnnotateQuery<TEntity>(
            [NotNull] this IQueryable<TEntity> source,
            [NotNull] QueryAnnotation annotation)
            where TEntity : class
            => QueryableHelpers.CreateQuery(source, s => s.AnnotateQuery(annotation));
    }
}
