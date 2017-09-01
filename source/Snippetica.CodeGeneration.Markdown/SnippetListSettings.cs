// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Snippetica.CodeGeneration.Markdown
{
    public class SnippetListSettings
    {
        public static SnippetListSettings Default { get; } = new SnippetListSettings();

        public SnippetListSettings(string engine = null, bool addHeading = true, bool addLinkToTitle = true)
        {
            Engine = engine;
            AddLinkToTitle = addLinkToTitle;
        }

        public string Engine { get; }

        public bool AddLinkToTitle { get; }
    }
}
