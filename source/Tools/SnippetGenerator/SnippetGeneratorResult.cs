// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Pihrtsoft.Snippets.CodeGeneration
{
    public class SnippetGeneratorResult
    {
        public SnippetGeneratorResult(IEnumerable<Snippet> snippets, string destinationDirectoryPath)
        {
            Snippets = snippets;
            DestinationDirectoryPath = destinationDirectoryPath;
        }

        public IEnumerable<Snippet> Snippets { get; }

        public string DestinationDirectoryPath { get; }

        public void Save()
        {
            IOUtility.SaveSnippets(Snippets.ToArray(), DestinationDirectoryPath);
        }
    }
}
