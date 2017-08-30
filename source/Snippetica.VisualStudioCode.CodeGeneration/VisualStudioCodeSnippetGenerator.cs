// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Pihrtsoft.Snippets;
using Snippetica.CodeGeneration.Commands;
using Snippetica.CodeGeneration.Json;
using Snippetica.CodeGeneration.Json.Package;
using Snippetica.CodeGeneration.Markdown;
using Snippetica.IO;
using Snippetica.Validations;

namespace Snippetica.CodeGeneration.VisualStudioCode
{
    public class VisualStudioCodeSnippetGenerator : SnippetGenerator
    {
        public VisualStudioCodeSnippetGenerator(LanguageDefinition languageDefinition)
            : base(languageDefinition)
        {
        }

        public static void GenerateSnippets(
            SnippetDirectory[] snippetDirectories,
            LanguageDefinition[] languageDefinitions,
            CharacterSequence[] characterSequences,
            string projectPath)
        {
            var snippets = new List<Snippet>();

            VisualStudioCodeSnippetGenerator[] generators = languageDefinitions.Select(f => new VisualStudioCodeSnippetGenerator(f)).ToArray();

            foreach (SnippetGeneratorResult result in GetResults(snippetDirectories, generators))
                snippets.AddRange(result.Snippets);

            snippets.AddRange(HtmlSnippetGenerator.GetResult(snippetDirectories).Snippets);
            snippets.AddRange(XmlSnippetGenerator.GetResult(snippetDirectories).Snippets);

            foreach (SnippetDirectory snippetDirectory in snippetDirectories
                .Where(f => f.IsRelease && !f.IsAutoGeneration && f.Language != Language.Xaml))
            {
                snippets.AddRange(snippetDirectory.EnumerateSnippets(SearchOption.TopDirectoryOnly));
            }

            snippets = ProcessSnippets(snippets).ToList();

            foreach (IGrouping<Language, Snippet> grouping in snippets.GroupBy(f => f.Language))
            {
                SnippetChecker.ThrowOnDuplicateFileName(grouping);
                SnippetChecker.ThrowOnDuplicateShortcut(grouping);

                Language language = grouping.Key;

                Console.WriteLine($"{language}: {grouping.Count()}");

                string languageId = LanguageHelper.GetVisualStudioCodeLanguageIdentifier(language);

                string fileName = Path.ChangeExtension(languageId, "json");

                string directoryName = $"Snippetica.{language}";
                string directoryPath = Path.Combine(projectPath, directoryName);

                IOUtility.WriteAllText(
                    Path.Combine(directoryPath, @"package\snippets", fileName),
                    JsonUtility.ToJsonText(grouping));

                IOUtility.SaveSnippets(grouping.ToArray(), directoryPath);

                var info = GetDefaultPackageInfo();
                info.Name += "-" + languageId;
                info.DisplayName += " for " + LanguageHelper.GetLanguageTitle(language);
                info.Description += LanguageHelper.GetLanguageTitle(language) + ".";
                info.Homepage += $"/{directoryName}/README.md";
                info.Keywords.AddRange(LanguageHelper.GetKeywords(language));
                info.Snippets.Add(new SnippetInfo() { Language = languageId, Path = $"./snippets/{languageId}.json" });

                IOUtility.WriteAllText(Path.Combine(directoryPath, "package", "package.json"), info.ToString());

                var snippetDirectory = new SnippetDirectory(directoryPath, language);

                MarkdownWriter.WriteDirectoryReadMe(snippetDirectory, characterSequences);
            }
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
                Homepage = "https://github.com/JosefPihrt/Snippetica/blob/master/source/Snippetica.VisualStudioCode",
                Repository = new RepositoryInfo()
                {
                    Type = "git",
                    Url = "https://github.com/JosefPihrt/Snippetica.git"
                },
                Bugs = new BugInfo() { Url = "https://github.com/JosefPihrt/Snippetica/issues" },
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
                    Debug.WriteLine(snippet.Title);

                    var shortcut = Regex.Match(snippet.Title, @"^\S+\s+").Value;

                    snippet.Title = snippet.Title.Substring(shortcut.Length);
                    snippet.Shortcut += "_" + shortcut.TrimEnd();

                    snippet.RemoveTag(KnownTags.TitleStartsWithShortcut);
                }

                if (snippet.HasTag(KnownTags.NonUniqueShortcut))
                {
                    var keyword = snippet.Keywords.FirstOrDefault(f => f.StartsWith(KnownTags.MetaShortcut));

                    if (keyword != null)
                    {
                        var shortcutSuffix = keyword.Substring(KnownTags.MetaShortcut.Length);

                        if (snippet.Shortcut.Last() != '_')
                            snippet.Shortcut += "_";

                        snippet.Shortcut += shortcutSuffix;

                        snippet.RemoveTag(KnownTags.NonUniqueShortcut);

                        snippet.Keywords.Remove(keyword);
                    }
                }

                yield return snippet;
            }
        }

        protected override void PostProcess(Snippet snippet)
        {
            LiteralCollection literals = snippet.Literals;

            Literal typeLiteral = literals[LiteralIdentifiers.Type];

            if (typeLiteral != null)
            {
                if (snippet.HasTag(KnownTags.GenerateVoidType))
                {
                    typeLiteral.DefaultValue = "void";
                }
                else
                {
                    typeLiteral.DefaultValue = "T";
                }
            }

            base.PostProcess(snippet);

            for (int i = 0; i < literals.Count; i++)
            {
                Literal literal = literals[i];

                if (!literal.IsEditable
                    && string.IsNullOrEmpty(literal.Function))
                {
                    snippet.RemoveLiteralAndReplacePlaceholders(literal.Identifier, literal.DefaultValue);
                }
            }
        }

        protected override IEnumerable<Command> GetTypeCommands(Snippet snippet)
        {
            if (snippet.HasTag(KnownTags.GenerateType)
                || snippet.HasTag(KnownTags.GenerateVoidType)
                || snippet.HasTag(KnownTags.GenerateBooleanType)
                || snippet.HasTag(KnownTags.GenerateDateTimeType)
                || snippet.HasTag(KnownTags.GenerateDoubleType)
                || snippet.HasTag(KnownTags.GenerateDecimalType)
                || snippet.HasTag(KnownTags.GenerateInt32Type)
                || snippet.HasTag(KnownTags.GenerateInt64Type)
                || snippet.HasTag(KnownTags.GenerateObjectType)
                || snippet.HasTag(KnownTags.GenerateStringType)
                || snippet.HasTag(KnownTags.GenerateSingleType))
            {
                yield return new TypeCommand(null);
            }
        }

        protected override IEnumerable<Command> GetImmutableCollectionCommands(Snippet snippet)
        {
            yield break;
        }

        protected override IEnumerable<Command> GetNonImmutableCollectionCommands(Snippet snippet)
        {
            yield break;
        }
    }
}
