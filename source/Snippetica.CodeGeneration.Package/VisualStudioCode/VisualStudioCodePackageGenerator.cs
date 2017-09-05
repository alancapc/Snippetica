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
using static Snippetica.KnownNames;
using static Snippetica.KnownPaths;

namespace Snippetica.CodeGeneration.Package.VisualStudioCode
{
    public class VisualStudioCodePackageGenerator : PackageGenerator
    {
        public VisualStudioCodePackageGenerator(SnippetEnvironment environment)
            : base(environment)
        {
        }

        protected override void SaveSnippets(List<Snippet> snippets, SnippetGeneratorResult result)
        {
            base.SaveSnippets(snippets, result);

            Language language = result.Language;

            string languageId = result.Language.GetIdentifier();

            string directoryPath = result.Path;

            string packageDirectoryPath = Path.Combine(directoryPath, "package");

            IOUtility.WriteAllText(
                Path.Combine(packageDirectoryPath, "snippets", Path.ChangeExtension(languageId, "json")),
                JsonUtility.ToJsonText(snippets.OrderBy(f => f.Title)));

            PackageInfo info = GetDefaultPackageInfo();
            info.Name += "-" + languageId;
            info.DisplayName += " for " + language.GetTitle();
            info.Description += language.GetTitle() + ".";
            info.Homepage += $"/{Path.GetFileName(directoryPath)}/{ReadMeFileName}";
            info.Keywords.AddRange(language.GetKeywords());
            info.Snippets.Add(new SnippetInfo() { Language = languageId, Path = $"./snippets/{languageId}.json" });

            IOUtility.WriteAllText(Path.Combine(packageDirectoryPath, "package.json"), info.ToString(), IOUtility.UTF8NoBom);

            SnippetListSettings settings = CreateSnippetListSettings(result);

            MarkdownWriter.WriteReadme(directoryPath, snippets, settings);

            settings.AddLinkToTitle = false;
            settings.Header = null;

            MarkdownWriter.WriteReadme(packageDirectoryPath, snippets, settings);
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
                Homepage = $"{SourceGitHubUrl}/{VisualStudioCodeExtensionProjectName}",
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

        protected override IEnumerable<Snippet> ProcessSnippets(List<Snippet> snippets)
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
