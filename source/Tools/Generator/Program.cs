// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Records;
using Pihrtsoft.Snippets;
using Pihrtsoft.Snippets.Comparers;
using Snippetica.CodeGeneration.Package;
using Snippetica.CodeGeneration.Package.VisualStudio;
using Snippetica.CodeGeneration.Package.VisualStudioCode;
using Snippetica.IO;
using static Snippetica.KnownPaths;

namespace Snippetica.CodeGeneration
{
    internal static class Program
    {
        private static readonly SnippetDeepEqualityComparer _snippetEqualityComparer = new SnippetDeepEqualityComparer();

        private static void Main(string[] args)
        {
            SnippetDirectory[] directories = LoadSnippetDirectories(@"..\..\SnippetDirectories.xml").ToArray();

            ShortcutInfo[] shortcuts = ShortcutInfo.LoadFromFile(@"..\..\Shortcuts.xml").ToArray();

            ShortcutInfo.SerializeToXml(Path.Combine(VisualStudioExtensionProjectPath, "Shortcuts.xml"), shortcuts);

            LoadLanguageDefinitions();

            SaveChangedSnippets(directories);

            GenerateSnippets(
                new VisualStudioEnvironment().GenerateSnippets(directories),
                new VisualStudioPackageGenerator(),
                VisualStudioExtensionProjectPath,
                shortcuts,
                KnownTags.ExcludeFromVisualStudio);

            GenerateSnippets(
                new VisualStudioCodeEnvironment().GenerateSnippets(directories),
                new VisualStudioCodePackageGenerator(),
                VisualStudioCodeExtensionProjectPath,
                shortcuts,
                KnownTags.ExcludeFromVisualStudioCode);

            Console.WriteLine("*** END ***");
            Console.ReadKey();
        }

        private static void GenerateSnippets(
            IEnumerable<SnippetGeneratorResult> results,
            PackageGenerator generator,
            string projectPath,
            ShortcutInfo[] shortcuts,
            string excludeTag)
        {
            generator.Shortcuts.AddRange(shortcuts.Where(f => !f.HasTag(excludeTag)));

            generator.GeneratePackageFiles(projectPath, results.Where(f => !f.SnippetDirectory.IsDevelopment));

            generator.GeneratePackageFiles(projectPath + KnownNames.DevSuffix, results.Where(f => f.SnippetDirectory.IsDevelopment));
        }

        private static void SaveChangedSnippets(SnippetDirectory[] directories)
        {
            foreach (SnippetDirectory directory in directories)
            {
                foreach (Snippet snippet in directory.EnumerateSnippets())
                {
                    var clone = (Snippet)snippet.Clone();

                    clone.SortCollections();

                    if (!_snippetEqualityComparer.Equals(snippet, clone))
                        IOUtility.SaveSnippet(clone, onlyIfChanged: false);
                }
            }
        }

        private static IEnumerable<SnippetDirectory> LoadSnippetDirectories(string url)
        {
            return Document.ReadRecords(url)
                .Where(f => !f.HasTag(KnownTags.Disabled))
                .Select(SnippetDirectoryMapper.MapFromRecord);
        }

        private static void LoadLanguageDefinitions()
        {
            LanguageDefinition[] languageDefinitions = Document.ReadRecords(@"..\..\LanguageDefinitions.xml")
                .Where(f => !f.HasTag(KnownTags.Disabled))
                .ToLanguageDefinitions()
                .ToArray();

            LanguageDefinition.CSharp = languageDefinitions.First(f => f.Language == Language.CSharp);
            LanguageDefinition.VisualBasic = languageDefinitions.First(f => f.Language == Language.VisualBasic);
        }
    }
}
