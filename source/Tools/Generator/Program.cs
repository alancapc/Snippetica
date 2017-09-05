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
            SnippetDirectory[] directories = LoadDirectories(@"..\..\Data\Directories.xml");

            ShortcutInfo[] shortcuts = ShortcutInfo.LoadFromFile(@"..\..\Data\Shortcuts.xml").ToArray();

            ShortcutInfo.SerializeToXml(Path.Combine(VisualStudioExtensionProjectPath, "Shortcuts.xml"), shortcuts);

            LoadLanguageDefinitions();

            SaveChangedSnippets(directories);

            var visualStudio = new VisualStudioEnvironment();

            GenerateSnippets(
                visualStudio,
                directories,
                new VisualStudioPackageGenerator(visualStudio),
                VisualStudioExtensionProjectPath,
                shortcuts,
                KnownTags.ExcludeFromVisualStudio);

            var visualStudioCode = new VisualStudioCodeEnvironment();

            GenerateSnippets(
                visualStudioCode,
                directories,
                new VisualStudioCodePackageGenerator(visualStudioCode),
                VisualStudioCodeExtensionProjectPath,
                shortcuts,
                KnownTags.ExcludeFromVisualStudioCode);

            Console.WriteLine("*** END ***");
            Console.ReadKey();
        }

        private static void GenerateSnippets(
            SnippetEnvironment environment,
            SnippetDirectory[] directories,
            PackageGenerator generator,
            string projectPath,
            ShortcutInfo[] shortcuts,
            string excludeTag)
        {
            generator.Shortcuts.AddRange(shortcuts.Where(f => !f.HasTag(excludeTag)));

            var results = new List<SnippetGeneratorResult>();
            var devResults = new List<SnippetGeneratorResult>();

            foreach (SnippetGeneratorResult result in environment.GenerateSnippets(directories))
            {
                if (result.IsDevelopment)
                {
                    devResults.Add(result);
                }
                else
                {
                    results.Add(result);
                }
            }

            generator.GeneratePackageFiles(projectPath, results);

            generator.GeneratePackageFiles(projectPath + KnownNames.DevSuffix, devResults);
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

        private static SnippetDirectory[] LoadDirectories(string url)
        {
            return Document.ReadRecords(url)
                .Where(f => !f.HasTag(KnownTags.Disabled))
                .Select(SnippetDirectoryMapper.MapFromRecord)
                .ToArray();
        }

        private static void LoadLanguageDefinitions()
        {
            LanguageDefinition[] languageDefinitions = Document.ReadRecords(@"..\..\Data\Languages.xml")
                .Where(f => !f.HasTag(KnownTags.Disabled))
                .ToLanguageDefinitions()
                .ToArray();

            LanguageDefinition.CSharp = languageDefinitions.First(f => f.Language == Language.CSharp);
            LanguageDefinition.VisualBasic = languageDefinitions.First(f => f.Language == Language.VisualBasic);
        }
    }
}
