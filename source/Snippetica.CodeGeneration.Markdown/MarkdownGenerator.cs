// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Pihrtsoft.Snippets;
using static Snippetica.KnownPaths;
using static Snippetica.KnownNames;

namespace Snippetica.CodeGeneration.Markdown
{
    public static class MarkdownGenerator
    {
        public static string GenerateSolutionReadMe(SnippetDirectory[] snippetDirectories)
        {
            using (var sw = new StringWriter())
            {
                sw.WriteLine($"## {ProductName}");
                sw.WriteLine();

                sw.WriteLine($"* {CodeGenerationUtility.GetProjectSubtitle(snippetDirectories)}");
                sw.WriteLine($"* [Release Notes]({MasterGitHubUrl}/{$"{ChangeLogFileName}"}).");
                sw.WriteLine($"* Browse all available snippets with [Snippet Browser]({GetSnippetBrowserUrl(Engine.VisualStudio)}).");
                sw.WriteLine();
                sw.WriteLine("### Distribution");
                sw.WriteLine();
                sw.WriteLine("* **Snippetica** is distributed as [Visual Studio Extension](http://marketplace.visualstudio.com/items?itemName=josefpihrt.Snippetica).");
                sw.WriteLine();
                sw.WriteLine("### Snippets");
                sw.WriteLine();

                sw.WriteLine("Folder|Count| |");
                sw.WriteLine("--- | --- | ---:");

                foreach (SnippetDirectory snippetDirectory in snippetDirectories)
                {
                    Snippet[] snippets = snippetDirectory.EnumerateSnippets().ToArray();

                    sw.WriteLine($"[{snippetDirectory.DirectoryName}]({VisualStudioExtensionGitHubUrl}/{snippetDirectory.DirectoryName}/{ReadMeFileName})|{snippets.Length}|[full list]({GetSnippetBrowserUrl(Engine.VisualStudio, snippetDirectory.Language)})");
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
                    sw.WriteLine($"* [{snippetDirectory.DirectoryName}]({snippetDirectory.DirectoryName}/{KnownNames.ReadMeFileName}) ({snippetDirectory.EnumerateSnippets().Count()} snippets)");
                }

                return sw.ToString();
            }
        }

        public static string GenerateDirectoryReadme(
            SnippetDirectory snippetDirectory,
            IList<ShortcutInfo> shortcuts,
            SnippetListSettings settings)
        {
            using (var sw = new StringWriter())
            {
                string directoryName = snippetDirectory.DirectoryName;

                if (settings.AddHeading)
                {
                    sw.WriteLine($"## {directoryName}");
                    sw.WriteLine();
                }

                if (!snippetDirectory.IsDevelopment)
                {
                    sw.WriteLine("### Snippet Browser");
                    sw.WriteLine();

                    sw.WriteLine($"* Browse all available snippets with [Snippet Browser]({GetSnippetBrowserUrl(settings.Engine, snippetDirectory.Language)}).");
                    sw.WriteLine();
                }

                if (!snippetDirectory.IsDevelopment
                    && !snippetDirectory.HasTag(KnownTags.NoQuickReference))
                {
                    shortcuts = shortcuts?
                        .Where(f => f.Languages.Contains(snippetDirectory.Language))
                        .ToList();

                    if (shortcuts?.Count > 0)
                    {
                        sw.WriteLine("### Quick Reference");
                        sw.WriteLine();

                        //TODO: ?
                        string filePath = $@"..\..\..\..\..\text\{directoryName}.md";

                        if (File.Exists(filePath))
                        {
                            sw.WriteLine(File.ReadAllText(filePath, Encoding.UTF8));
                            sw.WriteLine();
                        }

                        using (ShortcutInfoTableWriter tableWriter = ShortcutInfoTableWriter.Create())
                        {
                            tableWriter.WriteTable(shortcuts);
                            sw.Write(tableWriter.ToString());
                        }

                        sw.WriteLine();
                    }
                }

                sw.WriteLine("### List of Selected Snippets");
                sw.WriteLine();

                using (SnippetTableWriter tableWriter = (settings.AddLinkToTitle)
                    ? SnippetTableWriter.CreateTitleWithLinkThenShortcut(snippetDirectory.Path)
                    : SnippetTableWriter.CreateTitleThenShortcut())
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
