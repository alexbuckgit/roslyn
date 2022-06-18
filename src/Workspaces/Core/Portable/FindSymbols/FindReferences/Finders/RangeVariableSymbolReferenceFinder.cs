﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.CodeAnalysis.LanguageServices;

namespace Microsoft.CodeAnalysis.FindSymbols.Finders
{
    internal sealed class RangeVariableSymbolReferenceFinder : AbstractMemberScopedReferenceFinder<IRangeVariableSymbol>
    {
        protected override Func<FindReferencesDocumentState, SyntaxToken, string, bool> GetTokensMatchFunction()
            => static (state, token, name) => IdentifiersMatch(state.SyntaxFacts, name, token);
    }
}
