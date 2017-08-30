// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Snippetica.IO;

namespace Snippetica.CodeGeneration.Markdown
{
    public static class MarkdownWriter
    {
        public static void WriteSolutionReadMe(SnippetDirectory[] snippetDirectories, GeneralSettings settings)
        {
            IOUtility.WriteAllText(
                Path.Combine(settings.SolutionDirectoryPath, settings.ReadMeFileName),
                MarkdownGenerator.GenerateSolutionReadMe(snippetDirectories, settings));
        }

        public static void WriteProjectReadMe(SnippetDirectory[] snippetDirectories, string directoryPath)
        {
            IOUtility.WriteAllText(
                Path.Combine(directoryPath, "README.md"),
                MarkdownGenerator.GenerateProjectReadMe(snippetDirectories));
        }

        public static void WriteDirectoryReadMe(SnippetDirectory[] snippetDirectories, CharacterSequence[] characterSequences)
        {
            foreach (SnippetDirectory snippetDirectory in snippetDirectories)
                WriteDirectoryReadMe(snippetDirectory, characterSequences);
        }

        public static void WriteDirectoryReadMe(SnippetDirectory snippetDirectory, CharacterSequence[] characterSequences)
        {
            IOUtility.WriteAllText(
                Path.Combine(snippetDirectory.Path, "README.md"),
                MarkdownGenerator.GenerateDirectoryReadme(snippetDirectory, characterSequences));
        }
    }
}
