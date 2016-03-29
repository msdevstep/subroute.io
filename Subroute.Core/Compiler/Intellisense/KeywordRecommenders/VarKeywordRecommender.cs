// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;

namespace Subroute.Core.Compiler.Intellisense.KeywordRecommenders
{
    internal class VarKeywordRecommender : IKeywordRecommender<CSharpSyntaxContext>
    {
        public VarKeywordRecommender()
        {
        }

        private bool IsValidContext(CSharpSyntaxContext context)
        {
            if (context.IsStatementContext ||
                context.IsGlobalStatementContext)
            {
                return true;
            }

            return context.IsLocalVariableDeclarationContext;
        }

        public IEnumerable<RecommendedKeyword> RecommendKeywords(int position, CSharpSyntaxContext context, CancellationToken cancellationToken)
        {
            if (IsValidContext(context))
            {
                yield return new RecommendedKeyword("var");
            }
        }
    }
}
