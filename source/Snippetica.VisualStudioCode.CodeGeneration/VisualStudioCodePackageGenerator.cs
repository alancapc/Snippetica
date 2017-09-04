// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Pihrtsoft.Snippets;
using Snippetica.CodeGeneration.Json;
using Snippetica.CodeGeneration.Json.Package;
using Snippetica.CodeGeneration.Markdown;
using Snippetica.IO;
using Snippetica.Validations;
using static Snippetica.KnownPaths;

namespace Snippetica.CodeGeneration
{
    public static class VisualStudioCodePackageGenerator
    {
        public static void GeneratePackageFiles(
            IEnumerable<SnippetDirectory> snippetDirectories,
            string directoryPath,
            CharacterSequence[] characterSequences)
        {
            characterSequences = characterSequences.Where(f => !f.HasTag(KnownTags.ExcludeFromVisualStudioCode)).ToArray();

            var environment = new VisualStudioCodeEnvironment();

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
            string projectPath,
            CharacterSequence[] characterSequences)
        {
            var allSnippets = new List<Snippet>();

            foreach (SnippetGeneratorResult result in results)
            {
                List<Snippet> snippets = ProcessSnippets(result.Snippets).ToList();

                Validator.ThrowOnDuplicateFileName(snippets);

                Language language = result.SnippetDirectory.Language;

                string languageId = language.GetIdentifier();

                string directoryName = $"Snippetica.{language}";
                string directoryPath = Path.Combine(projectPath, directoryName);

                var snippetDirectory = new SnippetDirectory(directoryPath, language);

                Validator.ValidateSnippets(snippetDirectory);

                string packageDirectoryPath = Path.Combine(directoryPath, "package");

                IOUtility.WriteAllText(
                    Path.Combine(packageDirectoryPath, "snippets", Path.ChangeExtension(languageId, "json")),
                    JsonUtility.ToJsonText(snippets.OrderBy(f => f.Title)));

                IOUtility.SaveSnippets(snippets, directoryPath);

                PackageInfo info = GetDefaultPackageInfo();
                info.Name += "-" + languageId;
                info.DisplayName += " for " + language.GetTitle();
                info.Description += language.GetTitle() + ".";
                info.Homepage += $"/{directoryName}/{KnownNames.ReadMeFileName}";
                info.Keywords.AddRange(language.GetKeywords());
                info.Snippets.Add(new SnippetInfo() { Language = languageId, Path = $"./snippets/{languageId}.json" });

                IOUtility.WriteAllText(Path.Combine(packageDirectoryPath, "package.json"), info.ToString(), IOUtility.UTF8NoBom);

                IOUtility.WriteAllText(
                    Path.Combine(directoryPath, KnownNames.ReadMeFileName),
                    MarkdownGenerator.GenerateDirectoryReadme(snippetDirectory, characterSequences, SnippetListSettings.VisualStudioCode),
                    IOUtility.UTF8NoBom);

                IOUtility.WriteAllText(
                    Path.Combine(packageDirectoryPath, KnownNames.ReadMeFileName),
                    MarkdownGenerator.GenerateDirectoryReadme(snippetDirectory, characterSequences, new SnippetListSettings(Engine.VisualStudioCode, addHeading: false, addLinkToTitle: false)),
                    IOUtility.UTF8NoBom);

                allSnippets.AddRange(snippets);
            }

            IOUtility.SaveSnippetBrowserFile(allSnippets, Path.Combine(projectPath, "snippets.xml"));
        }

        private static PackageInfo GetDefaultPackageInfo()
        {
            var info = new PackageInfo()
            {
                Name = "snippetica",
                Publisher = "josefpihrt",
                DisplayName = "Snippetica",
                Description = "A collection of snippets for ",
                Icon = "images/icon.png",
                Version = "0.5.2",
                Author = "Josef Pihrt",
                License = "SEE LICENSE IN LICENSE.TXT",
                Homepage = $"{SourceGitHubUrl}/Snippetica.VisualStudioCode",
                Repository = new RepositoryInfo()
                {
                    Type = "git",
                    Url = $"{GitHubUrl}.git"
                },
                Bugs = new BugInfo() { Url = $"{GitHubUrl}/issues" },
                EngineVersion = "^1.0.0"
            };

            info.Categories.Add("Snippets");
            info.Keywords.Add("Snippet");
            info.Keywords.Add("Snippets");

            return info;
        }

        private static IEnumerable<Snippet> ProcessSnippets(IEnumerable<Snippet> snippets)
        {
            foreach (Snippet snippet in snippets)
            {
                if (snippet.HasTag(KnownTags.ExcludeFromVisualStudioCode))
                    continue;

                if (snippet.HasTag(KnownTags.TitleStartsWithShortcut))
                {
                    string shortcut = Regex.Match(snippet.Title, @"^\S+\s+").Value;

                    snippet.Title = snippet.Title.Substring(shortcut.Length);

                    shortcut = shortcut.TrimEnd();

                    if (shortcut != "-")
                    {
                        if (snippet.Shortcut.Last() != '_')
                            snippet.Shortcut += "_";

                        snippet.Shortcut += shortcut.TrimEnd();
                    }

                    snippet.RemoveTag(KnownTags.TitleStartsWithShortcut);
                }

                if (snippet.HasTag(KnownTags.NonUniqueShortcut))
                {
                    MetaValueInfo info = snippet.FindMetaValue(KnownTags.Shortcut);

                    if (info.Success)
                    {
                        if (snippet.Shortcut.Last() != '_')
                            snippet.Shortcut += "_";

                        snippet.Shortcut += info.Value;

                        snippet.Keywords.RemoveAt(info.KeywordIndex);

                        snippet.AddTag(KnownTags.ExcludeFromReadme);
                    }

                    snippet.RemoveTag(KnownTags.NonUniqueShortcut);
                }

                yield return snippet;
            }
        }
    }
}
