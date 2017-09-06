﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Pihrtsoft.Snippets;
using Snippetica.CodeGeneration.Markdown;
using Snippetica.IO;

namespace Snippetica.CodeGeneration.VisualStudio
{
    public class VisualStudioPackageGenerator : PackageGenerator
    {
        public VisualStudioPackageGenerator(SnippetEnvironment environment)
            : base(environment)
        {
        }

        public override void GeneratePackageFiles(string directoryPath, IEnumerable<SnippetGeneratorResult> results)
        {
            base.GeneratePackageFiles(directoryPath, results);

            IOUtility.WriteAllText(
                Path.Combine(directoryPath, "description.html"),
                HtmlGenerator.GenerateVisualStudioMarketplaceDescription(results));

            IOUtility.WriteAllText(
                Path.Combine(directoryPath, "regedit.pkgdef"),
                PkgDefGenerator.GeneratePkgDefFile(results));
        }

        protected override void SaveSnippets(List<Snippet> snippets, SnippetGeneratorResult result)
        {
            base.SaveSnippets(snippets, result);

            DirectoryReadmeSettings settings = Environment.CreateDirectoryReadmeSettings(result);

            MarkdownWriter.WriteDirectoryReadme(result.Path, snippets, settings);
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

        protected override IEnumerable<Snippet> PostProcess(List<Snippet> snippets)
        {
            foreach (Snippet snippet in base.PostProcess(snippets))
            {
                snippet.RemoveTag(KnownTags.ExcludeFromVisualStudioCode);

                if (snippet.TryGetMetaValue(KnownTags.Shortcut, out MetaValueInfo info))
                    snippet.Keywords.RemoveAt(info.KeywordIndex);

                yield return snippet;
            }
        }
    }
}
