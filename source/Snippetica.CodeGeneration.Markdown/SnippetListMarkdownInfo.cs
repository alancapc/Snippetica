// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Snippetica.CodeGeneration.Markdown
{
    public class SnippetListMarkdownInfo
    {
        public SnippetDirectory Directory { get; set; }

        public CharacterSequence[] CharacterSequences { get; set; }

        public string TitleSuffix { get; set; }

        public string AddReferenceToFullList { get; set; }

        public string QuickReferenceText { get; set; }
    }
}
