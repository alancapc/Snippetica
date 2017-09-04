// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets;
using Snippetica.IO;
using Snippetica.Validations;

namespace Snippetica.CodeGeneration.Package
{
    public class PackageGenerator
    {
        public List<ShortcutInfo> Shortcuts { get; } = new List<ShortcutInfo>();

        public virtual void GeneratePackageFiles(
            string directoryPath,
            IEnumerable<SnippetGeneratorResult> results)
        {
            var allSnippets = new List<Snippet>();

            foreach (SnippetGeneratorResult result in results)
            {
                List<Snippet> snippets = ProcessSnippets(result.Snippets).ToList();

                ValidateSnippets(snippets);

                SnippetDirectory snippetDirectory = result.SnippetDirectory;

                string subDirectoryPath = Path.Combine(directoryPath, snippetDirectory.DirectoryName);

                snippetDirectory = snippetDirectory.WithPath(subDirectoryPath);

                SaveSnippets(snippets, snippetDirectory);

                allSnippets.AddRange(snippets);
            }

            SaveAllSnippets(directoryPath, allSnippets);
        }

        protected virtual void SaveSnippets(List<Snippet> snippets, SnippetDirectory snippetDirectory)
        {
            IOUtility.SaveSnippets(snippets, snippetDirectory.Path);
        }

        protected virtual void SaveAllSnippets(string projectPath, List<Snippet> allSnippets)
        {
            IOUtility.SaveSnippetBrowserFile(allSnippets, Path.Combine(projectPath, "snippets.xml"));
        }

        protected virtual void ValidateSnippets(List<Snippet> snippets)
        {
            Validator.ValidateSnippets(snippets);

            Validator.ThrowOnDuplicateFileName(snippets);
        }

        protected virtual IEnumerable<Snippet> ProcessSnippets(List<Snippet> snippets)
        {
            return snippets;
        }
    }
}
