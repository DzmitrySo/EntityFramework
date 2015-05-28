// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Framework.Logging;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Relational.Query
{
    public abstract class RelationalCompositeMemberTranslator : IMemberTranslator
    {
        private List<IMemberTranslator> _relationalTranslators;

        public RelationalCompositeMemberTranslator([NotNull] ILoggerFactory loggerFactory)
        {
            _relationalTranslators = new List<IMemberTranslator>();
        }

        public virtual Expression Translate(MemberExpression expression)
        {
            foreach (var translator in Translators)
            {
                var translatedMember = translator.Translate(expression);
                if (translatedMember != null)
                {
                    return translatedMember;
                }
            }

            return null;
        }

        protected virtual IReadOnlyList<IMemberTranslator> Translators => _relationalTranslators;
    }
}
