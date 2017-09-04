// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Pihrtsoft.Snippets;
using Snippetica.CodeGeneration.Markdown;
using Snippetica.IO;
using Snippetica.Validations;

namespace Snippetica.CodeGeneration.VisualStudio
{
    public static class VisualStudioPackageGenerator
    {
        public static void GeneratePackageFiles(
            SnippetDirectory[] snippetDirectories,
            string directoryPath,
            CharacterSequence[] characterSequences)
        {
            var environment = new VisualStudioEnvironment();

            IEnumerable<SnippetGeneratorResult> results = environment.GenerateSnippets(snippetDirectories);

            GeneratePackageFiles(
                results.Where(f => !f.SnippetDirectory.IsDevelopment),
                directoryPath,
                characterSequences: characterSequences);

            GeneratePackageFiles(
                results.Where(f => f.SnippetDirectory.IsDevelopment),
                directoryPath + KnownNames.DevSuffix,
                characterSequences: null);
        }

        private static void GeneratePackageFiles(
            IEnumerable<SnippetGeneratorResult> results,
            string directoryPath,
            CharacterSequence[] characterSequences)
        {
            CopySnippetsToProject(directoryPath, results, characterSequences);

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

        public static void CopySnippetsToProject(
            string projectDirPath,
            IEnumerable<SnippetGeneratorResult> results,
            CharacterSequence[] characterSequences)
        {
            string projectName = Path.GetFileName(projectDirPath);

            string csprojPath = Path.Combine(projectDirPath, $"{projectName}.{ProjectDocument.CSharpProjectExtension}");

            var document = new ProjectDocument(csprojPath);

            document.RemoveSnippetFiles();

            var allSnippets = new List<Snippet>();

            XElement newItemGroup = document.AddItemGroup();

            foreach (SnippetGeneratorResult result in results)
            {
                string directoryPath = Path.Combine(projectDirPath, result.SnippetDirectory.DirectoryName);

                SnippetDirectory snippetDirectory = result.SnippetDirectory.WithPath(directoryPath);

                Directory.CreateDirectory(directoryPath);

                var snippets = new List<Snippet>();

                foreach (Snippet snippet in result.Snippets)
                {
                    snippet.RemoveTag(KnownTags.ExcludeFromVisualStudioCode);

                    MetaValueInfo info = snippet.FindMetaValue(KnownTags.Shortcut);

                    if (info.Success)
                        snippet.Keywords.RemoveAt(info.KeywordIndex);

                    snippets.Add(snippet);
                }

                Validator.ValidateSnippets(snippets);

                Validator.ThrowOnDuplicateFileName(snippets);

                IOUtility.SaveSnippets(snippets, directoryPath);

                document.AddSnippetFiles(snippets.Select(f => f.FilePath), newItemGroup);

                IOUtility.WriteAllText(
                    Path.Combine(directoryPath, KnownNames.ReadMeFileName),
                    MarkdownGenerator.GenerateDirectoryReadme(snippetDirectory, characterSequences, SnippetListSettings.VisualStudio));

                //TODO: 
                //IOUtility.UTF8NoBom);

                allSnippets.AddRange(snippets);
            }

            document.Save();

            IOUtility.SaveSnippetBrowserFile(allSnippets, Path.Combine(projectDirPath, "snippets.xml"));
        }
    }
}
