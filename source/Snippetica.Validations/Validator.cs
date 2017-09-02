// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets;
using Pihrtsoft.Snippets.Comparers;
using Pihrtsoft.Snippets.Validations;
using Snippetica.IO;

namespace Snippetica.Validations
{
    public static class Validator
    {
        private static readonly SnippetDeepEqualityComparer _snippetEqualityComparer = new SnippetDeepEqualityComparer();

        public static void ValidateSnippets(SnippetDirectory snippetDirectory)
        {
            Console.WriteLine();
            Console.WriteLine($"validating snippets in '{snippetDirectory.Path}' ...");

            CheckDuplicateShortcuts(snippetDirectory);

            ValidateSnippetsCore(snippetDirectory);
        }

        public static void ValidateSnippets(IEnumerable<SnippetDirectory> snippetDirectories)
        {
            foreach (IGrouping<Language, SnippetDirectory> grouping in snippetDirectories
                .GroupBy(f => f.Language))
            {
                Console.WriteLine();
                Console.WriteLine($"validating snippets with language '{grouping.Key}' ...");

                SnippetDirectory[] directories = grouping.ToArray();

                CheckDuplicateShortcuts(directories);

                directories = directories
                    .Where(f => !f.HasTag(KnownTags.VisualStudio))
                    .ToArray();

                ValidateSnippetsCore(directories);
            }
        }

        private static void ValidateSnippetsCore(SnippetDirectory snippetDirectory)
        {
            ValidateSnippets(snippetDirectory.EnumerateSnippets());
        }

        private static void ValidateSnippetsCore(SnippetDirectory[] snippetDirectories)
        {
            ValidateSnippets(snippetDirectories.SelectMany(f => f.EnumerateSnippets()));
        }

        public static void ValidateSnippets(IEnumerable<Snippet> snippets)
        {
            ValidateSnippets(snippets.ToList());
        }

        public static void ValidateSnippets(List<Snippet> snippets)
        {
            Console.WriteLine();
            Console.WriteLine($"number of snippets: {snippets.Count}");

            foreach (SnippetValidationResult result in Validate(snippets))
            {
                Console.WriteLine();
                Console.WriteLine($"{result.Importance.ToString().ToUpper()}: \"{result.Description}\" in \"{result.Snippet.FilePath}\"");
            }

            foreach (IGrouping<string, Snippet> snippet in snippets
                .Where(f => f.HasTag(KnownTags.NonUniqueShortcut))
                .GroupBy(f => f.Shortcut)
                .Where(f => f.Count() == 1))
            {
                Console.WriteLine();
                Console.WriteLine($"UNUSED TAG {KnownTags.NonUniqueShortcut} in \"{snippet.First().FilePath}\"");
            }

            foreach (Snippet snippet in snippets.Select(CloneAndSortCollections))
                IOUtility.SaveSnippet(snippet);
        }

        public static void CheckDuplicateShortcuts(SnippetDirectory snippetDirectory)
        {
            CheckDuplicateShortcuts(snippetDirectory.EnumerateSnippets());
        }

        public static void CheckDuplicateShortcuts(SnippetDirectory[] snippetDirectories)
        {
            CheckDuplicateShortcuts(snippetDirectories.SelectMany(f => f.EnumerateSnippets()));
        }

        public static void CheckDuplicateShortcuts(IEnumerable<Snippet> snippets)
        {
            CheckDuplicateShortcuts(snippets.ToList());
        }

        public static void CheckDuplicateShortcuts(List<Snippet> snippets)
        {
            foreach (DuplicateShortcutInfo info in FindDuplicateShortcuts(snippets, KnownTags.NonUniqueShortcut))
            {
                Console.WriteLine();
                Console.WriteLine($"DUPLICATE SHORTCUT: {info.Shortcut}");

                foreach (Snippet item in info.Snippets)
                    Console.WriteLine($"  {item.FilePath}");
            }
        }

        public static IEnumerable<DuplicateShortcutInfo> FindDuplicateShortcuts(IEnumerable<Snippet> snippets, string allowDuplicateKeyword)
        {
            foreach (DuplicateShortcutInfo info in SnippetUtility.FindDuplicateShortcuts(snippets))
            {
                if (!string.IsNullOrEmpty(info.Shortcut))
                {
                    if (allowDuplicateKeyword == null
                        || info.Snippets.Any(f => !f.HasTag(allowDuplicateKeyword)))
                    {
                        yield return info;
                    }
                }
            }
        }

        public static IEnumerable<SnippetValidationResult> Validate(IEnumerable<Snippet> snippets)
        {
            var validator = new CustomSnippetValidator();

            foreach (Snippet snippet in snippets)
            {
                foreach (SnippetValidationResult result in validator.Validate(snippet))
                    yield return result;
            }
        }

        private static Snippet CloneAndSortCollections(Snippet snippet)
        {
            var clone = (Snippet)snippet.Clone();

            clone.Literals.Sort();
            clone.Keywords.Sort();
            clone.Namespaces.Sort();
            clone.AlternativeShortcuts.Sort();

            return clone;
        }

        private static Snippet GetChangedSnippetOrDefault(Snippet snippet)
        {
            Snippet snippet2 = CloneAndSortCollections(snippet);

            if (!_snippetEqualityComparer.Equals(snippet, snippet2))
                return snippet2;

            return null;
        }

        public static void ThrowOnDuplicateFileName(IEnumerable<Snippet> snippets)
        {
            foreach (IGrouping<string, Snippet> grouping in snippets
                .GroupBy(f => Path.GetFileNameWithoutExtension(f.FilePath))
                .Where(f => f.CountExceeds(1)))
            {
                throw new InvalidOperationException($"Multiple snippets with same file name '{grouping.Key}'");
            }
        }

        public static void ThrowOnDuplicateShortcut(IEnumerable<Snippet> snippets)
        {
            foreach (IGrouping<string, Snippet> grouping in snippets
                .GroupBy(f => f.Shortcut)
                .Where(f => f.CountExceeds(1)))
            {
                throw new InvalidOperationException($"Multiple snippets with same shortcut '{grouping.Key}'");
            }
        }
    }
}
