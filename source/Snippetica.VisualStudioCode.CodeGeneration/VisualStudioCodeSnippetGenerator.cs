// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
                .Where(f => f.IsRelease && !f.IsAutoGeneration && !f.HasTag(KnownTags.ExcludeFromVisualStudioCode)))
            {
                snippets.AddRange(snippetDirectory.EnumerateSnippets(SearchOption.TopDirectoryOnly));
            }

            snippets = ProcessSnippets(snippets).ToList();

            foreach (IGrouping<Language, Snippet> grouping in snippets.GroupBy(f => f.Language))
            {
                SnippetChecker.ThrowOnDuplicateFileName(grouping);
                SnippetChecker.ThrowOnDuplicateShortcut(grouping);

                Language language = grouping.Key;

                string languageId = language.GetVisualStudioCodeIdentifier();

                string directoryName = $"Snippetica.{language}";
                string directoryPath = Path.Combine(projectPath, directoryName);

                string packageDirectoryPath = Path.Combine(directoryPath, "package");

                IOUtility.WriteAllText(
                    Path.Combine(packageDirectoryPath, "snippets", Path.ChangeExtension(languageId, "json")),
                    JsonUtility.ToJsonText(grouping));

                IOUtility.SaveSnippets(grouping.ToArray(), directoryPath);

                PackageInfo info = GetDefaultPackageInfo();
                info.Name += "-" + languageId;
                info.DisplayName += " for " + language.GetTitle();
                info.Description += language.GetTitle() + ".";
                info.Homepage += $"/{directoryName}/README.md";
                info.Keywords.AddRange(language.GetKeywords());
                info.Snippets.Add(new SnippetInfo() { Language = languageId, Path = $"./snippets/{languageId}.json" });

                IOUtility.WriteAllText(Path.Combine(packageDirectoryPath, "package.json"), info.ToString(), IOUtility.UTF8NoBom);

                var snippetDirectory = new SnippetDirectory(directoryPath, language);

                IOUtility.WriteAllText(
                    Path.Combine(directoryPath, "README.md"),
                    MarkdownGenerator.GenerateDirectoryReadme(snippetDirectory, characterSequences, new SnippetListSettings("VisualStudioCode")),
                    IOUtility.UTF8NoBom);

                IOUtility.WriteAllText(
                    Path.Combine(packageDirectoryPath, "README.md"),
                    MarkdownGenerator.GenerateDirectoryReadme(snippetDirectory, characterSequences, new SnippetListSettings("VisualStudioCode", addHeading: false, addLinkToTitle: false)),
                    IOUtility.UTF8NoBom);
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
                    string keyword = snippet.Keywords.FirstOrDefault(f => f.StartsWith(KnownTags.MetaShortcut));

                    if (keyword != null)
                    {
                        string shortcutSuffix = keyword.Substring(KnownTags.MetaShortcut.Length);

                        if (snippet.Shortcut.Last() != '_')
                            snippet.Shortcut += "_";

                        snippet.Shortcut += shortcutSuffix;

                        snippet.Keywords.Remove(keyword);

                        snippet.RemoveTag(KnownTags.NonUniqueShortcut);

                        snippet.AddTag(KnownTags.ExcludeFromReadme);
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

                if (!literal.IsEditable)
                {
                    if (string.IsNullOrEmpty(literal.Function))
                    {
                        snippet.RemoveLiteralAndReplacePlaceholders(literal.Identifier, literal.DefaultValue);
                    }
                    else
                    {
                        literal.IsEditable = true;
                        literal.Function = null;
                    }
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
