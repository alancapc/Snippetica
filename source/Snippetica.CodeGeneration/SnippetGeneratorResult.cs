﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Pihrtsoft.Snippets;
using Snippetica.IO;

namespace Snippetica.CodeGeneration
{
    public class SnippetGeneratorResult
    {
        public static SnippetGeneratorResult Empty { get; } = new SnippetGeneratorResult(new Snippet[0], "");

    public SnippetGeneratorResult(IEnumerable<Snippet> snippets, string destinationDirectoryPath)
        {
            Snippets = snippets;
            DestinationDirectoryPath = destinationDirectoryPath;
        }

        public IEnumerable<Snippet> Snippets { get; }

        public string DestinationDirectoryPath { get; }

        public void Save()
        {
            if (!string.IsNullOrEmpty(DestinationDirectoryPath))
                IOUtility.SaveSnippets(Snippets.ToArray(), DestinationDirectoryPath);
        }
    }
}
