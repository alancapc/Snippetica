﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets;

namespace Snippetica.CodeGeneration
{
    public static class XmlSnippetGenerator
    {
        private static readonly Version _version = new Version(1, 1, 0);

        private const string AttributeIdentifier = "attribute";
        private const string CommentIdentifier = "comment";
        private const string ContentIdentifier = "content";
        private const string NameIdentifier = "name";
        private const string EndIdentifier = "end";

        public const string ElementShortcut = "e";
        public const string AttributeShortcut = "a";
        public const string SelfClosingShortcut = "s";
        public const string ContentShortcut = "x";
        public const string CommentShortcut = "c";
        public const string RegionShortcut = "r";

       public static IEnumerable<Snippet> GenerateSnippets(Language language)
        {
            foreach (Snippet snippet in GenerateSnippets())
            {
                snippet.Author = "Josef Pihrt";
                snippet.SnippetTypes = SnippetTypes.Expansion;
                snippet.AddTag(KnownTags.AutoGenerated);
                snippet.Language = language;
                snippet.FilePath = Path.ChangeExtension(snippet.FilePath, ".snippet");
                snippet.FormatVersion = _version;

                RemoveUnusedLiterals(snippet);
                snippet.Literals.Sort();

                yield return snippet;
            }
        }

        private static void RemoveUnusedLiterals(Snippet snippet)
        {
            for (int i = snippet.Literals.Count - 1; i >= 0; i--)
            {
                Literal literal = snippet.Literals[i];

                if (!snippet.Code.Placeholders.Contains(literal.Identifier))
                    snippet.RemoveLiteralAndPlaceholders(literal);
            }
        }

        private static IEnumerable<Snippet> GenerateSnippets()
        {
            yield return CreateCommentSnippet();

            Snippet snippet = CreateElementSnippet();

            yield return CreateElementSnippet((Snippet)snippet.Clone());
            yield return WithAttribute((Snippet)snippet.Clone());
            yield return WithContent((Snippet)snippet.Clone());
            yield return WithAttributeWithContent((Snippet)snippet.Clone());

            snippet = CreateSelfClosingElementSnippet(CreateSelfClosingElementSnippet());
            snippet.SuffixShortcut(SelfClosingShortcut);
            yield return snippet;

            snippet = WithAttribute(CreateSelfClosingElementSnippet());
            snippet.SuffixShortcut(SelfClosingShortcut);
            yield return snippet;
        }

        private static Snippet CreateElementSnippet(Snippet snippet)
        {
            snippet.ReplacePlaceholders(AttributeIdentifier, "");
            snippet.ReplacePlaceholders(EndIdentifier, "");
            snippet.ReplacePlaceholders(ContentIdentifier, $"${EndIdentifier}$");

            return snippet;
        }

        private static Snippet CreateSelfClosingElementSnippet(Snippet snippet)
        {
            snippet.ReplacePlaceholders(AttributeIdentifier, "");

            return snippet;
        }

        private static Snippet CreateElementSnippet()
        {
            var s = new Snippet()
            {
                Title = "element",
                Shortcut = ElementShortcut
            };

            s.Description = s.Title;

            Literal nameLiteral = CreateElementNameLiteral();
            s.AddLiteral(nameLiteral);

            Literal attributeLiteral = CreateAttributeLiteral();
            s.AddLiteral(attributeLiteral);

            Literal contentLiteral = CreateContentLiteral();
            s.AddLiteral(contentLiteral);

            var w = new SnippetCodeWriter();
            w.Write("<");
            w.WritePlaceholder(nameLiteral.Identifier);
            w.WritePlaceholder(attributeLiteral.Identifier);
            w.Write(">");
            w.WritePlaceholder(contentLiteral.Identifier);
            w.Write("</");
            w.WritePlaceholder(NameIdentifier);
            w.Write(">");
            w.WriteEndPlaceholder();
            s.CodeText = w.ToString();

            s.FilePath = "Element";

            return s;
        }

        private static Snippet CreateSelfClosingElementSnippet()
        {
            var s = new Snippet()
            {
                Title = "self-closing element",
                Shortcut = ElementShortcut
            };

            s.Description = s.Title;

            Literal nameLiteral = CreateElementNameLiteral();
            s.AddLiteral(nameLiteral);

            Literal attributeLiteral = CreateAttributeLiteral();
            s.AddLiteral(attributeLiteral);

            var w = new SnippetCodeWriter();
            w.Write("<");
            w.WritePlaceholder(nameLiteral.Identifier);
            w.WritePlaceholder(attributeLiteral.Identifier);
            w.Write(" />");
            w.WriteEndPlaceholder();
            s.CodeText = w.ToString();

            s.FilePath = "SelfClosingElement";

            return s;
        }

        private static Snippet CreateCommentSnippet()
        {
            var s = new Snippet()
            {
                Title = "comment",
                Shortcut = CommentShortcut
            };

            s.Description = s.Title;

            Literal commentLiteral = CreateCommentLiteral();
            s.AddLiteral(commentLiteral);

            var w = new SnippetCodeWriter();
            w.Write("<!-- ");
            w.WritePlaceholder(commentLiteral.Identifier);
            w.Write(" -->");
            w.WriteEndPlaceholder();
            s.CodeText = w.ToString();

            s.FilePath = "Comment";

            return s;
        }

        private static Snippet WithAttribute(Snippet s)
        {
            s.SuffixTitle(" (with attribute)");
            s.SuffixShortcut(AttributeShortcut);
            s.SuffixDescription(" (with attribute)");

            s.ReplacePlaceholders(AttributeIdentifier, $" ${AttributeIdentifier}$");

            if (s.Code.Placeholders.Contains(ContentIdentifier))
            {
                s.ReplacePlaceholders(EndIdentifier, "");
                s.ReplacePlaceholders(ContentIdentifier, $"${EndIdentifier}$");
            }

            s.SuffixFileName("WithAttribute");

            return s;
        }

        private static Snippet WithContent(Snippet s)
        {
            s.SuffixTitle(" (with content)");
            s.SuffixShortcut(ContentShortcut);
            s.SuffixDescription(" (with content)");

            s.ReplacePlaceholders(AttributeIdentifier, "");

            s.SuffixFileName("WithContent");

            return s;
        }

        private static Snippet WithAttributeWithContent(Snippet s)
        {
            s.SuffixTitle(" (with attribute, with content)");
            s.SuffixShortcut(AttributeShortcut + ContentShortcut);
            s.SuffixDescription(" (with attribute, with content)");

            s.ReplacePlaceholders(AttributeIdentifier, $" ${AttributeIdentifier}$");

            s.SuffixFileName("WithAttributeWithContent");

            return s;
        }

        private static Literal CreateAttributeLiteral()
        {
            return new Literal(AttributeIdentifier, "Attribute(s)", "attribute=\"\"");
        }

        private static Literal CreateContentLiteral()
        {
            return new Literal(ContentIdentifier, "Content", ContentIdentifier);
        }

        private static Literal CreateElementNameLiteral()
        {
            return new Literal(NameIdentifier, "Element name", "x");
        }

        private static Literal CreateCommentLiteral()
        {
            return new Literal(CommentIdentifier, "Comment", CommentIdentifier);
        }
    }
}
