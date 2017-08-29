﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Pihrtsoft.Snippets;
using Snippetica.CodeGeneration.Markdown;
using Snippetica.IO;

namespace Snippetica.CodeGeneration.VisualStudio
{
    public static class VisualStudioPackageGenerator
    {
        public static void GenerateVisualStudioPackageFiles(
            SnippetDirectory[] directories,
            CharacterSequence[] characterSequences,
            GeneralSettings settings)
        {
            CopySnippetsToProject(settings.ExtensionProjectPath, directories);

            directories = directories
                .Select(f => f.WithPath(Path.Combine(settings.ExtensionProjectPath, f.DirectoryName)))
                .ToArray();

            MarkdownWriter.WriteProjectReadMe(directories, settings.ExtensionProjectPath);

            MarkdownWriter.WriteDirectoryReadMe(directories, characterSequences, settings);

            IOUtility.WriteAllText(
                Path.Combine(settings.ExtensionProjectPath, settings.GalleryDescriptionFileName),
                HtmlGenerator.GenerateVisualStudioGalleryDescription(directories, settings));

            IOUtility.WriteAllText(
                Path.Combine(settings.ExtensionProjectPath, settings.PkgDefFileName),
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

                Snippet[] snippets = snippetDirectory
                    .EnumerateSnippets()
                    .Select(f => f.RemoveTag(KnownTags.ExcludeFromVisualStudioCode))
                    .ToArray();

                foreach (IGrouping<string, Snippet> grouping in snippets
                    .GroupBy(f => Path.GetFileNameWithoutExtension(f.FilePath))
                    .Where(f => f.Count() > 1))
                {
                    throw new Exception($"multiple files with same name '{grouping.Key}'");
                }

                IOUtility.SaveSnippets(snippets, directoryPath);

                document.AddSnippetFiles(snippets.Select(f => f.FilePath), newItemGroup);

#if !DEBUG
                allSnippets.AddRange(snippets);
#endif
            }

            document.Save();

#if !DEBUG
            foreach (Snippet snippet in allSnippets)
            {
                string submenuShortcut = snippet.GetSubmenuShortcut();

                snippet.RemoveShortcutFromTitle();

                snippet.RemoveMetaKeywords();
                snippet.Keywords.Add($"{KnownTags.MetaTagPrefix}Name:{snippet.FileNameWithoutExtension()}");

                if (!string.IsNullOrEmpty(submenuShortcut))
                    snippet.Keywords.Add($"{KnownTags.MetaTagPrefix}SubmenuShortcut:{submenuShortcut}");
            }

            IOUtility.SaveSnippetsToSingleFile(
                allSnippets
                    .Where(f => !f.HasTag(KnownTags.ExcludeFromReadme))
                    .OrderBy(f => f.Language.ToString())
                    .ThenBy(f => f.FileNameWithoutExtension()),
                Path.Combine(projectDirPath, "snippets.xml"));
#endif
        }
    }
}