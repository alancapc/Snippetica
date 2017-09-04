// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Pihrtsoft.Snippets;
using Snippetica.CodeGeneration.Commands;
using static Pihrtsoft.Text.RegularExpressions.Linq.Patterns;

namespace Snippetica.CodeGeneration
{
    public class XamlSnippetGenerator : SnippetGenerator
    {
        private static readonly Regex _regex = AssertBack(LetterLower()).Assert(LetterUpper()).ToRegex();

        public override IEnumerable<Snippet> GenerateSnippets(string sourceDirectoryPath, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return base.GenerateSnippets(sourceDirectoryPath, searchOption);

            //TODO: 
            //.Concat(XmlSnippetGenerator.GenerateSnippets(Language.Xaml));
        }

        protected override JobCollection CreateJobs(Snippet snippet)
        {
            var jobs = new JobCollection();

            if (snippet.HasTag(KnownTags.GenerateAlternativeShortcut))
            {
                jobs.AddCommand(new SimpleCommand(f => f.Shortcut = f.Shortcut.ToLowerInvariant(), CommandKind.ShortcutToLowercase));
                jobs.AddCommand(new AlternativeShortcutCommand(CreateAlternativeShortcut(snippet)));
            }

            return jobs;

            //if (snippet.HasTag(KnownTags.GenerateXamlProperty))
            //    jobs.AddCommand(new XamlPropertyCommand());
        }

        protected override Snippet PostProcess(Snippet snippet)
        {
            snippet.RemoveTag(KnownTags.NonUniqueTitle);

            return base.PostProcess(snippet);
        }

        private static string CreateAlternativeShortcut(Snippet snippet)
        {
            IEnumerable<string> values = _regex.Split(snippet.Shortcut)
                .Select(f => f.Substring(0, 1) + f.Substring(f.Length - 1, 1))
                .Select(f => f.ToLower());

            return string.Concat(values);
        }
    }
}
