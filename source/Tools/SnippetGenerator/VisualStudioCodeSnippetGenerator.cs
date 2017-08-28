﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Pihrtsoft.Snippets.CodeGeneration.Commands;

namespace Pihrtsoft.Snippets.CodeGeneration
{
    public class VisualStudioCodeSnippetGenerator : SnippetGenerator
    {
        public VisualStudioCodeSnippetGenerator(LanguageDefinition languageDefinition)
            : base(languageDefinition)
        {
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
