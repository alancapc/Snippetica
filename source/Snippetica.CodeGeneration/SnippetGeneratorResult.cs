// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Pihrtsoft.Snippets;

namespace Snippetica.CodeGeneration
{
    public class SnippetGeneratorResult
    {
        public SnippetGeneratorResult(List<Snippet> snippets, SnippetDirectory snippetDirectory)
        {
            Snippets = snippets;
            SnippetDirectory = snippetDirectory;
        }

        public List<Snippet> Snippets { get; }

        public SnippetDirectory SnippetDirectory { get; }
    }
}
