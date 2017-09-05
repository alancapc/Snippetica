// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Pihrtsoft.Snippets;
using Snippetica.CodeGeneration.Commands;

namespace Snippetica.CodeGeneration
{
    public class LanguageSnippetGenerator : SnippetGenerator
    {
        public LanguageSnippetGenerator(LanguageDefinition languageDefinition)
        {
            LanguageDefinition = languageDefinition;
        }

        public LanguageDefinition LanguageDefinition { get; }

        protected override JobCollection CreateJobs(Snippet snippet)
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

            return jobs;
        }

        protected override ExecutionContext CreateExecutionContext(Snippet snippet)
        {
            return new LanguageExecutionContext((Snippet)snippet.Clone(), LanguageDefinition);
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

        protected override Snippet PostProcess(Snippet snippet)
        {
            base.PostProcess(snippet);

            if (snippet.Language == Language.VisualBasic)
                snippet.ReplaceSubOrFunctionLiteral("Function");

            snippet.Title = snippet.Title
                .ReplacePlaceholder(Placeholders.Type, " ", true)
                .ReplacePlaceholder(Placeholders.OfType, " ", true)
                .ReplacePlaceholder(Placeholders.GenericType, LanguageDefinition.GetTypeParameterList("T"));

            snippet.Description = snippet.Description
                .ReplacePlaceholder(Placeholders.Type, " ", true)
                .ReplacePlaceholder(Placeholders.OfType, " ", true)
                .ReplacePlaceholder(Placeholders.GenericType, LanguageDefinition.GetTypeParameterList("T"));

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

            return snippet;
        }
    }
}
