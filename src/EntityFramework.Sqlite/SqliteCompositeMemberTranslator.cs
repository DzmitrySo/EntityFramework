﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Framework.Logging;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteCompositeMemberTranslator : RelationalCompositeMemberTranslator
    {
        private List<IMemberTranslator> _sqliteTranslators;

        public SqliteCompositeMemberTranslator([NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            _sqliteTranslators = new List<IMemberTranslator>();
        }

        protected override IReadOnlyList<IMemberTranslator> Translators
            => base.Translators.Concat(_sqliteTranslators).ToList();
    }
}
