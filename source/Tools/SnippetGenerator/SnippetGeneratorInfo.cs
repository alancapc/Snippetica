// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Pihrtsoft.Snippets.CodeGeneration
{
    public class SnippetGeneratorInfo
    {
        public SnippetGeneratorInfo(SnippetGenerator generator, string sourcePath, string destinationPath)
        {
            Generator = generator;
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
        }

        public SnippetGenerator Generator { get; }

        public string SourcePath { get; }

        public string DestinationPath { get; }

        public static IEnumerable<SnippetGeneratorInfo> CreateMany(SnippetDirectory[] snippetDirectories, SnippetGenerator[] snippetGenerators, Func<SnippetDirectory, bool> predicate)
        {
            foreach (SnippetGenerator snippetGenerator in snippetGenerators)
            {
                SnippetGeneratorInfo info = Create(
                    snippetDirectories
                        .Where(f => f.Language == snippetGenerator.LanguageDefinition.Language && predicate(f))
                        .ToArray(),
                    snippetGenerator);

                if (info != null)
                    yield return info;
            }
        }

        private static SnippetGeneratorInfo Create(SnippetDirectory[] snippetDirectories, SnippetGenerator snippetGenerator)
        {
            if (snippetDirectories.Length == 0)
                return null;

            string source = snippetDirectories
                .Where(f => f.HasTag(KnownTags.AutoGenerationSource))
                .Select(f => f.Path)
                .FirstOrDefault();

            if (source == null)
                return null;

            string destination = snippetDirectories
                .Where(f => f.HasTag(KnownTags.AutoGenerationDestination))
                .Select(f => f.Path)
                .FirstOrDefault();

            if (destination == null)
                return null;

            return new SnippetGeneratorInfo(snippetGenerator, source, destination);
        }
    }
}
