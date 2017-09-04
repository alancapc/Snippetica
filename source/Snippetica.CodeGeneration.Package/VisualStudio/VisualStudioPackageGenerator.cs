// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Pihrtsoft.Snippets;
using Snippetica.CodeGeneration.Markdown;
using Snippetica.CodeGeneration.Package;
using Snippetica.IO;

namespace Snippetica.CodeGeneration.Package.VisualStudio
{
    public class VisualStudioPackageGenerator : PackageGenerator
    {
        public override void GeneratePackageFiles(string directoryPath, IEnumerable<SnippetGeneratorResult> results)
        {
            base.GeneratePackageFiles(directoryPath, results);

            SnippetDirectory[] directories = results
                .Select(f => f.SnippetDirectory.WithPath(Path.Combine(directoryPath, f.SnippetDirectory.DirectoryName)))
                .ToArray();

            MarkdownWriter.WriteProjectReadMe(directories, directoryPath);

            IOUtility.WriteAllText(
                Path.Combine(directoryPath, "description.html"),
                HtmlGenerator.GenerateVisualStudioMarketplaceDescription(directories));

            IOUtility.WriteAllText(
                Path.Combine(directoryPath, "regedit.pkgdef"),
                PkgDefGenerator.GeneratePkgDefFile(directories));
        }

        protected override void SaveSnippets(List<Snippet> snippets, SnippetDirectory snippetDirectory)
        {
            base.SaveSnippets(snippets, snippetDirectory);

            //TODO: 
            List<ShortcutInfo> shortcuts = (snippetDirectory.IsDevelopment)
                ? null
                : Shortcuts;

            IOUtility.WriteAllText(
                Path.Combine(snippetDirectory.Path, KnownNames.ReadMeFileName),
                MarkdownGenerator.GenerateDirectoryReadme(snippetDirectory, Shortcuts, SnippetListSettings.VisualStudio));
        }

        protected override void SaveAllSnippets(string projectPath, List<Snippet> allSnippets)
        {
            base.SaveAllSnippets(projectPath, allSnippets);

            string projectName = Path.GetFileName(projectPath);

            string csprojPath = Path.Combine(projectPath, $"{projectName}.{ProjectDocument.CSharpProjectExtension}");

            var document = new ProjectDocument(csprojPath);

            document.RemoveSnippetFiles();

            XElement newItemGroup = document.AddItemGroup();

            document.AddSnippetFiles(allSnippets.Select(f => f.FilePath), newItemGroup);

            document.Save();
        }

        protected override IEnumerable<Snippet> ProcessSnippets(List<Snippet> snippets)
        {
            foreach (Snippet snippet in snippets)
            {
                snippet.RemoveTag(KnownTags.ExcludeFromVisualStudioCode);

                MetaValueInfo info = snippet.FindMetaValue(KnownTags.Shortcut);

                if (info.Success)
                    snippet.Keywords.RemoveAt(info.KeywordIndex);

                yield return snippet;
            }
        }
    }
}
