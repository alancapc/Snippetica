// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets;
using Snippetica.CodeGeneration.Commands;

namespace Snippetica.CodeGeneration
{
    public class XamlSnippetGenerator
    {
        public static SnippetGeneratorResult GetResult(SnippetDirectory[] snippetDirectories)
        {
            IEnumerable<SnippetDirectory> directories = snippetDirectories
                .Where(f => f.Language == Language.Xaml);

            string sourceDirPath = directories.First(f => f.IsAutoGenerationSource).Path;
            string destinationDirPath = directories.First(f => f.IsAutoGenerationDestination).Path;

            var snippets = new List<Snippet>();

            snippets.AddRange(XmlSnippetGenerator.GenerateSnippets(destinationDirPath, Language.Xaml));

            var generator = new XamlSnippetGenerator();
            snippets.AddRange(generator.GenerateSnippets(sourceDirPath));

            return new SnippetGeneratorResult(snippets, destinationDirPath);
        }

        public IEnumerable<Snippet> GenerateSnippets(string sourceDirectoryPath)
        {
            return SnippetSerializer.Deserialize(sourceDirectoryPath, SearchOption.AllDirectories)
                .SelectMany(snippet => GenerateSnippets(snippet));
        }

        public IEnumerable<Snippet> GenerateSnippets(Snippet snippet)
        {
            var jobs = new JobCollection();

            if (snippet.HasTag(KnownTags.GenerateAlternativeShortcut))
            {
                jobs.AddCommand(new SimpleCommand(f => f.Shortcut = f.Shortcut.ToLowerInvariant(), CommandKind.ShortcutToLowercase));
                jobs.AddCommand(new AlternativeShortcutCommand());
            }

            //if (snippet.HasTag(KnownTags.GenerateXamlProperty))
            //    jobs.AddCommand(new XamlPropertyCommand());

            foreach (Job job in jobs)
            {
                var context = new ExecutionContext((Snippet)snippet.Clone());

                job.Execute(context);

                if (!context.IsCanceled)
                {
                    foreach (Snippet snippet2 in context.Snippets)
                    {
                        snippet2.SortCollections();
                        yield return snippet2;
                    }
                }
            }
        }
    }
}
