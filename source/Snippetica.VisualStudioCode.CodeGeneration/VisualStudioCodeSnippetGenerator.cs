// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets;
using Snippetica.CodeGeneration.Commands;
using Snippetica.IO;

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
            string projectPath,
            Func<SnippetDirectory, bool> predicate)
        {
            var snippets = new List<Snippet>();

            VisualStudioCodeSnippetGenerator[] generators = languageDefinitions.Select(f => new VisualStudioCodeSnippetGenerator(f)).ToArray();

            foreach (SnippetGeneratorResult result in SnippetGenerator.GetResults(snippetDirectories, generators, predicate))
                snippets.AddRange(result.Snippets);

            snippets.AddRange(HtmlSnippetGenerator.GetResult(snippetDirectories).Snippets);
            snippets.AddRange(XmlSnippetGenerator.GetResult(snippetDirectories).Snippets);

            snippets = snippets
                .Where(f => !f.HasTag(KnownTags.ExcludeFromVisualStudioCode))
                .Select(f => f.RemoveShortcutFromTitle())
                .ToList();

            foreach (Snippet snippet in snippets)
                snippet.RemoveMetaKeywords();

            foreach (IGrouping<Language, Snippet> grouping in snippets.GroupBy(f => f.Language))
            {
                Console.WriteLine($"{grouping.Key}: {grouping.Count()}");

                string fileName = Path.ChangeExtension(LanguageHelper.GetVisualStudioCodeLanguageIdentifier(grouping.Key), "json");

                string filePath = Path.Combine(projectPath, @"package\snippets", fileName);

                string content = JsonUtility.ToJson(grouping);

                IOUtility.WriteAllText(filePath, content);

                IOUtility.SaveSnippets(grouping.ToArray(), Path.Combine(projectPath, $"Snippetica.{grouping.Key}"));
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
