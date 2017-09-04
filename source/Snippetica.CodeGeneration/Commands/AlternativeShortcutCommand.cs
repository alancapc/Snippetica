// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Pihrtsoft.Snippets;

namespace Snippetica.CodeGeneration.Commands
{
    public class AlternativeShortcutCommand : BaseCommand
    {
        public AlternativeShortcutCommand(string shortcut)
        {
            Shortcut = shortcut;
        }

        public string Shortcut { get; }

        public override CommandKind Kind
        {
            get { return CommandKind.AlternativeShortcut; }
        }

        public override Command ChildCommand
        {
            get { return new SimpleCommand(f => f.SuffixFileName("_"), CommandKind.SuffixFileName); }
        }

        protected override void Execute(ExecutionContext context, Snippet snippet)
        {
            snippet.Shortcut = Shortcut;

            snippet.AddTag(KnownTags.NonUniqueTitle);
        }
    }
}
