// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets;
using Snippetica.IO;
using Snippetica.Validations;

namespace Snippetica.CodeGeneration
{
    public class PackageGenerator
    {
        public PackageGenerator(SnippetEnvironment environment)
        {
            Environment = environment;
        }

        public SnippetEnvironment Environment { get; }

        public virtual void GeneratePackageFiles(
            string directoryPath,
            IEnumerable<SnippetGeneratorResult> results)
        {
            var allSnippets = new List<Snippet>();

            foreach (SnippetGeneratorResult result in results)
            {
                result.Path = Path.Combine(directoryPath, result.DirectoryName);

                List<Snippet> snippets = PostProcess(result.Snippets).ToList();

                ValidateSnippets(snippets);

                SaveSnippets(snippets, result);

                allSnippets.AddRange(snippets);
            }

            SaveAllSnippets(directoryPath, allSnippets);
        }

        protected virtual void SaveSnippets(List<Snippet> snippets, SnippetGeneratorResult result)
        {
            IOUtility.SaveSnippets(snippets, result.Path);
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

        protected virtual IEnumerable<Snippet> PostProcess(List<Snippet> snippets)
        {
            foreach (Snippet snippet in snippets)
            {
                if (snippet.TryGetTag(KnownTags.Environment, out TagInfo info))
                {
                    if (string.Equals(info.Value, Environment.Kind.GetIdentifier()))
                    {
                        snippet.Keywords.RemoveAt(info.KeywordIndex);
                    }
                    else
                    {
                        continue;
                    }
                }

                if (snippet.HasTag(KnownTags.NonUniqueTitle))
                {
                    snippet.Title += " _";
                    snippet.RemoveTag(KnownTags.NonUniqueTitle);
                }

                CheckObsoleteSnippet(snippet);

                snippet.SortCollections();

                snippet.Author = "Josef Pihrt";

                if (snippet.SnippetTypes == SnippetTypes.None)
                    snippet.SnippetTypes = SnippetTypes.Expansion;

                yield return snippet;
            }
        }

        private static void CheckObsoleteSnippet(Snippet snippet)
        {
            if (snippet.TryGetTag(KnownTags.Obsolete, out TagInfo info))
            {
                string s = $"Shortcut '{snippet.Shortcut}' is obsolete, use '{info.Value}' instead.";

                if (snippet.Language == Language.CSharp)
                {
                    s = $"/* {s} */";
                }
                else if (snippet.Language == Language.VisualBasic)
                {
                    s = $"' {s}\r\n";
                }
                else
                {
                    throw new NotSupportedException(snippet.Language.ToString());
                }

                snippet.CodeText += s;

                snippet.Keywords.RemoveAt(info.KeywordIndex);
                snippet.AddTag(KnownTags.ExcludeFromSnippetBrowser);
            }
        }
    }
}
