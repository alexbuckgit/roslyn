﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.EmbeddedLanguages.RegularExpressions;
using Microsoft.CodeAnalysis.Options;

namespace Microsoft.CodeAnalysis.Editor
{
    internal readonly record struct BraceMatchingOptions(
        bool HighlightRelatedRegexComponentsUnderCursor)
    {
        public static BraceMatchingOptions From(Project project)
            => From(project.Solution.Options, project.Language);

        public static BraceMatchingOptions From(OptionSet options, string language)
            => new(
                HighlightRelatedRegexComponentsUnderCursor: options.GetOption(RegularExpressionsOptions.HighlightRelatedRegexComponentsUnderCursor, language));
    }
}
