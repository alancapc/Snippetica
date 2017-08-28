﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets.CodeGeneration.Commands;

namespace Pihrtsoft.Snippets.CodeGeneration
{
    public class SnippetGenerator
    {
        public SnippetGenerator(LanguageDefinition languageDefinition)
        {
            LanguageDefinition = languageDefinition;
        }

        public LanguageDefinition LanguageDefinition { get; }

        public static IEnumerable<SnippetGeneratorResult> GetResults(SnippetDirectory[] snippetDirectories, SnippetGenerator[] snippetGenerators, Func<SnippetDirectory, bool> predicate)
        {
            foreach (SnippetGeneratorInfo info in SnippetGeneratorInfo.CreateMany(snippetDirectories, snippetGenerators, predicate))
            {
                IEnumerable<Snippet> snippets = SnippetSerializer.Deserialize(info.SourcePath, SearchOption.AllDirectories)
                    .SelectMany(info.Generator.GenerateSnippets);

                yield return new SnippetGeneratorResult(snippets, info.DestinationPath);
            }
        }

        public IEnumerable<Snippet> GenerateSnippets(Snippet snippet)
        {
            var jobs = new JobCollection();

            jobs.AddCommands(GetTypeCommands(snippet));

            if (snippet.HasTag(KnownTags.GenerateCollection))
                jobs.AddCommands(GetNonImmutableCollectionCommands(snippet));

            if (snippet.HasTag(KnownTags.GenerateImmutableCollection))
                jobs.AddCommands(GetImmutableCollectionCommands(snippet));

            jobs.AddCommands(GetAccessModifierCommands(snippet));

            if (snippet.HasTag(KnownTags.GenerateStaticModifier))
                jobs.AddCommand(CommandUtility.StaticCommand);

            if (snippet.HasTag(KnownTags.GenerateVirtualModifier))
                jobs.AddCommand(CommandUtility.VirtualCommand);

            if (snippet.HasTag(KnownTags.GenerateInitializer))
                jobs.AddCommand(CommandUtility.InitializerCommand);

            if (snippet.HasTag(KnownTags.GenerateParameters))
                jobs.AddCommand(CommandUtility.ParametersCommand);

            if (snippet.HasTag(KnownTags.GenerateArguments))
                jobs.AddCommand(CommandUtility.ArgumentsCommand);

            if (snippet.HasTag(KnownTags.GenerateUnchanged))
                jobs.Add(new Job());

            foreach (Job job in jobs)
            {
                var context = new LanguageExecutionContext((Snippet)snippet.Clone(), LanguageDefinition);

                job.Execute(context);

                if (!context.IsCanceled)
                {
                    foreach (Snippet snippet2 in context.Snippets)
                    {
                        PostProcess(snippet2);
                        yield return snippet2;
                    }
                }
            }
        }

        protected virtual IEnumerable<Command> GetTypeCommands(Snippet snippet)
        {
            return CommandUtility.GetTypeCommands(snippet, LanguageDefinition);
        }

        protected virtual IEnumerable<Command> GetNonImmutableCollectionCommands(Snippet snippet)
        {
            return CommandUtility.GetNonImmutableCollectionCommands(LanguageDefinition);
        }

        protected virtual IEnumerable<Command> GetImmutableCollectionCommands(Snippet snippet)
        {
            return CommandUtility.GetImmutableCollectionCommands(LanguageDefinition);
        }

        protected virtual IEnumerable<Command> GetAccessModifierCommands(Snippet snippet)
        {
            return CommandUtility.GetAccessModifierCommands(snippet, LanguageDefinition);
        }

        protected virtual void PostProcess(Snippet snippet)
        {
            ReplacePlaceholders(snippet);

            if (snippet.Language == Language.VisualBasic)
                snippet.ReplaceSubOrFunctionLiteral("Function");

            RemoveUnusedLiterals(snippet);

            RemoveKeywords(snippet);

            snippet.AddTag(KnownTags.AutoGenerated);

            snippet.SortCollections();

            snippet.Author = "Josef Pihrt";

            if (snippet.SnippetTypes == SnippetTypes.None)
                snippet.SnippetTypes = SnippetTypes.Expansion;
        }

        private void ReplacePlaceholders(Snippet snippet)
        {
            snippet.Title = snippet.Title
                .ReplacePlaceholder(Placeholders.Type, " ", true)
                .ReplacePlaceholder(Placeholders.OfType, " ", true)
                .ReplacePlaceholder(Placeholders.GenericType, LanguageDefinition.GetTypeParameterList("T"));

            snippet.Description = snippet.Description
                .ReplacePlaceholder(Placeholders.Type, " ", true)
                .ReplacePlaceholder(Placeholders.OfType, " ", true)
                .ReplacePlaceholder(Placeholders.GenericType, LanguageDefinition.GetTypeParameterList("T"));
        }

        private void RemoveKeywords(Snippet snippet)
        {
            snippet.RemoveTags(LanguageDefinition.Types.Select(f => KnownTags.GenerateTypeTag(f.Name)));
            snippet.RemoveTags(LanguageDefinition.Modifiers.Select(f => KnownTags.GenerateModifierTag(f.Name)));

            snippet.RemoveTags(
                KnownTags.GenerateType,
                KnownTags.GenerateAccessModifier,
                KnownTags.GenerateInitializer,
                KnownTags.GenerateUnchanged,
                KnownTags.GenerateParameters,
                KnownTags.GenerateArguments,
                KnownTags.GenerateCollection,
                KnownTags.GenerateImmutableCollection,
                KnownTags.Array,
                KnownTags.Collection,
                KnownTags.Dictionary,
                KnownTags.TryParse,
                KnownTags.Initializer);
        }

        private static void RemoveUnusedLiterals(Snippet snippet)
        {
            for (int i = snippet.Literals.Count - 1; i >= 0; i--)
            {
                Literal literal = snippet.Literals[i];

                if (!literal.IsEditable
                    && string.IsNullOrEmpty(literal.DefaultValue))
                {
                    snippet.RemoveLiteralAndPlaceholders(literal);
                }
            }
        }
    }
}
