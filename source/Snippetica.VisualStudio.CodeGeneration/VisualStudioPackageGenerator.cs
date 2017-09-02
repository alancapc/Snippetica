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
        public static object PropertyName { get; }

        public static void GenerateVisualStudioPackageFiles(
            string directoryPath,
            SnippetDirectory[] directories,
            CharacterSequence[] characterSequences)
        {
            CopySnippetsToProject(directoryPath, directories);

            directories = directories
                .Select(f => f.WithPath(Path.Combine(directoryPath, f.DirectoryName)))
                .ToArray();

            MarkdownWriter.WriteProjectReadMe(directories, directoryPath);

            MarkdownWriter.WriteDirectoryReadMe(directories, characterSequences, SnippetListSettings.VisualStudio);

            IOUtility.WriteAllText(
                Path.Combine(directoryPath, "description.html"),
                HtmlGenerator.GenerateVisualStudioGalleryDescription(directories));

            IOUtility.WriteAllText(
                Path.Combine(directoryPath, "regedit.pkgdef"),
                PkgDefGenerator.GeneratePkgDefFile(directories));
        }

        public static void CopySnippetsToProject(string projectDirPath, IEnumerable<SnippetDirectory> snippetDirectories)
        {
            string projectName = Path.GetFileName(projectDirPath);

            string csprojPath = Path.Combine(projectDirPath, $"{projectName}.{ProjectDocument.CSharpProjectExtension}");

            var document = new ProjectDocument(csprojPath);

            document.RemoveSnippetFiles();

#if !DEBUG
            var allSnippets = new List<Snippet>();
#endif

            XElement newItemGroup = document.AddItemGroup();

            foreach (SnippetDirectory snippetDirectory in snippetDirectories)
            {
                string directoryPath = Path.Combine(projectDirPath, snippetDirectory.DirectoryName);

                Directory.CreateDirectory(directoryPath);

                var snippets = new List<Snippet>();

                foreach (Snippet snippet in snippetDirectory.EnumerateSnippets())
                {
                    snippet.RemoveTag(KnownTags.ExcludeFromVisualStudioCode);

                    string keyword = snippet.Keywords.FirstOrDefault(f => f.StartsWith(KnownTags.MetaShortcut));

                    if (keyword != null)
                        snippet.Keywords.Remove(keyword);

                    snippets.Add(snippet);
                }

                Validator.ThrowOnDuplicateFileName(snippets);

                IOUtility.SaveSnippets(snippets, directoryPath);

                document.AddSnippetFiles(snippets.Select(f => f.FilePath), newItemGroup);

#if !DEBUG
                allSnippets.AddRange(snippets);
#endif
            }

            document.Save();

#if !DEBUG
            IOUtility.SaveSnippetBrowserFile(allSnippets, Path.Combine(projectDirPath, "snippets.xml"));
#endif
        }
    }
}
