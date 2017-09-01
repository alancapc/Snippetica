// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Pihrtsoft.Snippets;

namespace Snippetica.CodeGeneration.Markdown
{
    public static class MarkdownGenerator
    {
        public static string GenerateSolutionReadMe(SnippetDirectory[] snippetDirectories, GeneralSettings settings)
        {
            using (var sw = new StringWriter())
            {
                sw.WriteLine("## Snippetica");
                sw.WriteLine();

                sw.WriteLine($"* {settings.GetProjectSubtitle(snippetDirectories)}");
                sw.WriteLine($"* [Release Notes]({settings.GitHubMasterPath}/{$"{settings.ChangeLogFileName}"}).");
                sw.WriteLine("* [Browse and Search All Snippets](http://pihrt.net/Snippetica/Snippets).");
                sw.WriteLine();
                sw.WriteLine("### Distribution");
                sw.WriteLine();
                sw.WriteLine("* **Snippetica** is distributed as [Visual Studio Extension](http://visualstudiogallery.msdn.microsoft.com/a5576f35-9f87-4c9c-8f1f-059421a23aed).");
                sw.WriteLine();
                sw.WriteLine("### Snippets");
                sw.WriteLine();

                sw.WriteLine("Folder|Count| |");
                sw.WriteLine("--- | --- | ---:");

                foreach (SnippetDirectory snippetDirectory in snippetDirectories)
                {
                    Snippet[] snippets = snippetDirectory.EnumerateSnippets().ToArray();

                    sw.WriteLine($"[{snippetDirectory.DirectoryName}]({settings.GitHubExtensionProjectPath}/{snippetDirectory.DirectoryName}/{settings.ReadMeFileName})|{snippets.Length}|[full list](http://pihrt.net/Snippetica/Snippets?Language={snippetDirectory.Language})");
                }

                return sw.ToString();
            }
        }

        public static string GenerateProjectReadMe(SnippetDirectory[] snippetDirectories)
        {
            using (var sw = new StringWriter())
            {
                sw.WriteLine();

                foreach (SnippetDirectory snippetDirectory in snippetDirectories)
                {
                    sw.WriteLine($"* [{snippetDirectory.DirectoryName}]({snippetDirectory.DirectoryName}/README.md) ({snippetDirectory.EnumerateSnippets().Count()} snippets)");
                }

                return sw.ToString();
            }
        }

        public static string GenerateDirectoryReadme(
            SnippetDirectory snippetDirectory,
            CharacterSequence[] characterSequences,
            bool addLinkToTitle = true)
        {
            using (var sw = new StringWriter())
            {
                string directoryName = snippetDirectory.DirectoryName;

                sw.WriteLine($"## {directoryName}");
                sw.WriteLine();

                if (!snippetDirectory.IsDev
                    && !snippetDirectory.HasTag(KnownTags.NoQuickReference))
                {
                    characterSequences = characterSequences?
                        .Where(f => f.Languages.Contains(snippetDirectory.Language))
                        .ToArray();

                    if (characterSequences?.Length > 0)
                    {
                        sw.WriteLine("### Quick Reference");
                        sw.WriteLine();

                        //TODO: 
                        string filePath = $@"..\..\..\..\..\text\{directoryName}.md";

                        if (File.Exists(filePath))
                        {
                            sw.WriteLine(File.ReadAllText(filePath, Encoding.UTF8));
                            sw.WriteLine();
                        }

                        using (CharacterSequenceTableWriter tableWriter = CharacterSequenceTableWriter.Create())
                        {
                            tableWriter.WriteTable(characterSequences);
                            sw.Write(tableWriter.ToString());
                        }

                        sw.WriteLine();
                    }
                }

                if (!snippetDirectory.IsDev)
                {
                    sw.WriteLine($"* [full list of snippets](http://pihrt.net/Snippetica/Snippets?Language={snippetDirectory.Language})");
                    sw.WriteLine();
                }

                sw.WriteLine("### List of Selected Snippets");
                sw.WriteLine();

                using (SnippetTableWriter tableWriter = (addLinkToTitle)
                    ? SnippetTableWriter.CreateTitleWithLinkThenShortcut(snippetDirectory.Path)
                    : SnippetTableWriter.CreateTitleThenShortcut(snippetDirectory.Path))
                {
                    IEnumerable<Snippet> snippets = snippetDirectory
                        .EnumerateSnippets()
                        .Where(f => !f.HasTag(KnownTags.ExcludeFromReadme));

                    tableWriter.WriteTable(snippets);
                    sw.Write(tableWriter.ToString());
                }

                return sw.ToString();
            }
        }

        private static string GenerateSnippetList(Snippet[] snippets, string directoryPath, SnippetTableWriter tableWriter)
        {
            using (var sw = new StringWriter())
            {
                sw.WriteLine($"## {Path.GetFileName(directoryPath)}");
                sw.WriteLine();

                string s = $"* {snippets.Length} snippets";
                sw.WriteLine(s);

                sw.WriteLine();
                sw.WriteLine("### List of Snippets");
                sw.WriteLine();

                tableWriter.WriteTable(snippets);
                sw.Write(tableWriter.ToString());

                return sw.ToString();
            }
        }
    }
}
