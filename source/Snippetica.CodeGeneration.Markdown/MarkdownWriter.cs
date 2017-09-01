// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Snippetica.IO;
using static Snippetica.KnownPaths;
using static Snippetica.KnownNames;

namespace Snippetica.CodeGeneration.Markdown
{
    public static class MarkdownWriter
    {
        public static void WriteSolutionReadMe(SnippetDirectory[] snippetDirectories)
        {
            IOUtility.WriteAllText(
                Path.Combine(SolutionDirectoryPath, ReadMeFileName),
                MarkdownGenerator.GenerateSolutionReadMe(snippetDirectories));
        }

        public static void WriteProjectReadMe(SnippetDirectory[] snippetDirectories, string directoryPath)
        {
            IOUtility.WriteAllText(
                Path.Combine(directoryPath, ReadMeFileName),
                MarkdownGenerator.GenerateProjectReadMe(snippetDirectories));
        }

        public static void WriteDirectoryReadMe(
            SnippetDirectory[] snippetDirectories,
            CharacterSequence[] characterSequences,
            SnippetListSettings settings)
        {
            foreach (SnippetDirectory snippetDirectory in snippetDirectories)
                WriteDirectoryReadMe(snippetDirectory, characterSequences, settings);
        }

        public static void WriteDirectoryReadMe(
            SnippetDirectory snippetDirectory,
            CharacterSequence[] characterSequences,
            SnippetListSettings settings)
        {
            IOUtility.WriteAllText(
                Path.Combine(snippetDirectory.Path, ReadMeFileName),
                MarkdownGenerator.GenerateDirectoryReadme(snippetDirectory, characterSequences, settings));
        }
    }
}
