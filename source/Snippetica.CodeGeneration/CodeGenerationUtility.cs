// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Snippetica.CodeGeneration
{
    //TODO: CodeGenerationUtility
    public static class CodeGenerationUtility
    {
        public static string GetProjectSubtitle(SnippetDirectory[] snippetDirectories)
        {
            return $"A collection of snippets for {GetLanguagesSeparatedWithComma(snippetDirectories)}.";
        }

        private static string GetLanguagesSeparatedWithComma(SnippetDirectory[] snippetDirectories)
        {
            string[] languages = snippetDirectories
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

        public static string GetProjectSubtitle(SnippetGeneratorResult[] snippetDirectories)
        {
            return $"A collection of snippets for {GetLanguagesSeparatedWithComma(snippetDirectories)}.";
        }

        private static string GetLanguagesSeparatedWithComma(SnippetGeneratorResult[] snippetDirectories)
        {
            string[] languages = snippetDirectories
                .GroupBy(f => f.SnippetDirectory.Language.GetTitle())
                .Select(f => f.Key)
                .ToArray();

            for (int i = 1; i < languages.Length - 1; i++)
            {
                languages[i] = ", " + languages[i];
            }

            languages[languages.Length - 1] = " and " + languages[languages.Length - 1];

            return string.Concat(languages);
        }
    }
}
