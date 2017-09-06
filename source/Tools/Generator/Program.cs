// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Records;
using Pihrtsoft.Snippets;
using Pihrtsoft.Snippets.Comparers;
using Snippetica.CodeGeneration.Markdown;
using Snippetica.CodeGeneration.VisualStudio;
using Snippetica.CodeGeneration.VisualStudioCode;
using Snippetica.IO;
using static Snippetica.KnownNames;
using static Snippetica.KnownPaths;

namespace Snippetica.CodeGeneration
{
    internal static class Program
    {
        private static readonly SnippetDeepEqualityComparer _snippetEqualityComparer = new SnippetDeepEqualityComparer();

        private static readonly ShortcutInfo[] _shortcuts = ShortcutInfo.LoadFromFile(@"..\..\Data\Shortcuts.xml").ToArray();

        private static void Main(string[] args)
        {
            SnippetDirectory[] directories = LoadDirectories(@"..\..\Data\Directories.xml");

            ShortcutInfo.SerializeToXml(Path.Combine(VisualStudioExtensionProjectPath, "Shortcuts.xml"), _shortcuts);

            LoadLanguageDefinitions();

            SaveChangedSnippets(directories);

            var visualStudio = new VisualStudioEnvironment();

            List<SnippetGeneratorResult> visualStudioResults = GenerateSnippets(
                visualStudio,
                directories,
                VisualStudioExtensionProjectPath,
                KnownTags.ExcludeFromVisualStudio);

            var visualStudioCode = new VisualStudioCodeEnvironment();

            List<SnippetGeneratorResult> visualStudioCodeResults = GenerateSnippets(
                visualStudioCode,
                directories,
                VisualStudioCodeExtensionProjectPath,
                KnownTags.ExcludeFromVisualStudioCode);

            using (var sw = new StringWriter())
            {
                sw.WriteLine($"## {ProductName}");
                sw.WriteLine();

                IEnumerable<Language> languages = visualStudioResults
                    .Concat(visualStudioCodeResults)
                    .Select(f => f.Language).Distinct();

                sw.WriteLine($"* {CodeGenerationUtility.GetProjectSubtitle(languages)}");
                sw.WriteLine($"* [Release Notes]({MasterGitHubUrl}/{$"{ChangeLogFileName}"}).");
                sw.WriteLine();

                MarkdownGenerator.GenerateProjectReadme(visualStudioResults, sw, visualStudio.CreateProjectReadmeSettings());

                sw.WriteLine();

                MarkdownGenerator.GenerateProjectReadme(visualStudioCodeResults, sw, visualStudioCode.CreateProjectReadmeSettings());

                IOUtility.WriteAllText(Path.Combine(SolutionDirectoryPath, ReadMeFileName), sw.ToString(), IOUtility.UTF8NoBom);
            }

            Console.WriteLine("*** END ***");
            Console.ReadKey();
        }

        private static List<SnippetGeneratorResult> GenerateSnippets(
            SnippetEnvironment environment,
            SnippetDirectory[] directories,
            string projectPath,
            string excludeTag)
        {
            environment.Shortcuts.AddRange(_shortcuts.Where(f => !f.HasTag(excludeTag)));

            PackageGenerator generator = environment.CreatePackageGenerator();

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

            generator.GeneratePackageFiles(projectPath + DevSuffix, devResults);

            MarkdownWriter.WriteProjectReadme(projectPath, results, environment.CreateProjectReadmeSettings());

            return results;
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
