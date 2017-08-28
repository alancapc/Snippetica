// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Pihrtsoft.Snippets.CodeGeneration
{
    public class VisualStudioSnippetGenerator : SnippetGenerator
    {
        public VisualStudioSnippetGenerator(LanguageDefinition languageDefinition)
            : base(languageDefinition)
        {
        }

        protected override void PostProcess(Snippet snippet)
        {
            Literal typeLiteral = snippet.Literals[LiteralIdentifiers.Type];

            if (typeLiteral != null)
                typeLiteral.DefaultValue = "T";

            base.PostProcess(snippet);

            snippet.RemoveTag(KnownTags.ExcludeFromVisualStudioCode);
        }
    }
}
