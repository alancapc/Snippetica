// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets;
using static Snippetica.CodeGeneration.CodeGenerationUtility;
using static Snippetica.KnownNames;
using static Snippetica.KnownPaths;

namespace Snippetica.CodeGeneration.Markdown
{
    public static class MarkdownGenerator
    {
        public static string GenerateSolutionReadMe(SnippetGeneratorResult[] results)
        {
            using (var sw = new StringWriter())
            {
                sw.WriteLine($"## {ProductName}");
                sw.WriteLine();

                sw.WriteLine($"* {GetProjectSubtitle(results)}");
                sw.WriteLine($"* [Release Notes]({MasterGitHubUrl}/{$"{ChangeLogFileName}"}).");
                sw.WriteLine($"* Browse all available snippets with [Snippet Browser]({GetSnippetBrowserUrl(EnvironmentKind.VisualStudio)}).");
                sw.WriteLine();
                sw.WriteLine("### Distribution");
                sw.WriteLine();
                sw.WriteLine("* **Snippetica** is distributed as [Visual Studio Extension](http://marketplace.visualstudio.com/items?itemName=josefpihrt.Snippetica).");
                sw.WriteLine();
                sw.WriteLine("### Snippets");
                sw.WriteLine();

                sw.WriteLine("Folder|Count| |");
                sw.WriteLine("--- | --- | ---:");

                foreach (SnippetGeneratorResult result in results)
                {
                    sw.WriteLine($"[{result.DirectoryName}]({VisualStudioExtensionGitHubUrl}/{result.DirectoryName}/{ReadMeFileName})|{result.Snippets.Count}|[full list]({GetSnippetBrowserUrl(EnvironmentKind.VisualStudio, result.Language)})");
                }

                return sw.ToString();
            }
        }

        public static string GenerateProjectReadMe(IEnumerable<SnippetGeneratorResult> results)
        {
            using (var sw = new StringWriter())
            {
                sw.WriteLine();

                foreach (SnippetGeneratorResult result in results)
                {
                    sw.WriteLine($"* [{result.DirectoryName}]({result.DirectoryName}/{ReadMeFileName}) ({result.Snippets.Count} snippets)");
                }

                return sw.ToString();
            }
        }

        public static string GenerateDirectoryReadme(
            IEnumerable<Snippet> snippets,
            SnippetListSettings settings)
        {
            using (var sw = new StringWriter())
            {
                if (!string.IsNullOrEmpty(settings.Header))
                {
                    sw.WriteLine($"## {settings.Header}");
                    sw.WriteLine();
                }

                if (!settings.IsDevelopment)
                {
                    sw.WriteLine("### Snippet Browser");
                    sw.WriteLine();

                    sw.WriteLine($"* Browse all available snippets with [Snippet Browser]({GetSnippetBrowserUrl(settings.Environment.Kind, settings.Language)}).");
                    sw.WriteLine();
                }

                if (!settings.IsDevelopment
                    && settings.AddQuickReference)
                {
                    List<ShortcutInfo> shortcuts = settings.Shortcuts
                        .Where(f => f.Languages.Contains(settings.Language))
                        .ToList();

                    if (shortcuts.Count > 0)
                    {
                        sw.WriteLine("### Quick Reference");
                        sw.WriteLine();

                        if (settings.QuickReferenceText != null)
                        {
                            sw.WriteLine(settings.QuickReferenceText);
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
                    ? SnippetTableWriter.CreateTitleWithLinkThenShortcut(settings.DirectoryPath)
                    : SnippetTableWriter.CreateTitleThenShortcut())
                {
                    snippets = snippets.Where(f => !f.HasTag(KnownTags.ExcludeFromReadme));

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
