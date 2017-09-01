// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Snippetica.CodeGeneration.Markdown
{
    public class SnippetListSettings
    {
        public static SnippetListSettings VisualStudio { get; } = new SnippetListSettings(Engine.VisualStudio);

        public static SnippetListSettings VisualStudioCode { get; } = new SnippetListSettings(Engine.VisualStudioCode);

        public SnippetListSettings(Engine engine, bool addHeading = true, bool addLinkToTitle = true)
        {
            Engine = engine;
            AddHeading = addHeading;
            AddLinkToTitle = addLinkToTitle;
        }

        public Engine Engine { get; }

        public bool AddHeading { get; }

        public bool AddLinkToTitle { get; }
    }
}
