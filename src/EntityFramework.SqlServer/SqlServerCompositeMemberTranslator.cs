// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.SqlServer.Query.Methods;
using Microsoft.Framework.Logging;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerCompositeMemberTranslator : RelationalCompositeMemberTranslator
    {
        private List<IMemberTranslator> _sqlServerTranslators;

        public SqlServerCompositeMemberTranslator([NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            _sqlServerTranslators = new List<IMemberTranslator>
            {
                new StringLengthTranslator(),
                new DateTimeNowTranslator(),
            };
        }

        protected override IReadOnlyList<IMemberTranslator> Translators
            => base.Translators.Concat(_sqlServerTranslators).ToList();
    }
}
