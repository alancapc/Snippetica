// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Pihrtsoft.Snippets;

namespace Snippetica.CodeGeneration
{
    public static class CodeGenerationUtility
    {
        public static string GetProjectSubtitle(IEnumerable<SnippetGeneratorResult> results)
        {
            return $"A collection of snippets for {GetLanguagesSeparatedWithComma(results)}.";
        }

        private static string GetLanguagesSeparatedWithComma(IEnumerable<SnippetGeneratorResult> results)
        {
            string[] languages = results
                .GroupBy(f => f.Language.GetTitle())
                .Select(f => f.Key)
                .ToArray();

            for (int i = 1; i < languages.Length - 1; i++)
            {
                languages[i] = ", " + languages[i];
            }

            languages[languages.Length - 1] = " and " + languages[languages.Length - 1];

            return string.Concat(languages);
        }

        public static string GetSnippetBrowserUrl(EnvironmentKind environmentKind, Language language = Language.None)
        {
            string s = $"?engine={environmentKind.GetIdentifier()}";

            if (language != Language.None)
                s += $"&language={language.GetIdentifier()}";

            return KnownPaths.SnippetBrowserUrl + s;
        }
    }
}
