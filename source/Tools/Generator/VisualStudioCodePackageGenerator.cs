// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pihrtsoft.Snippets.CodeGeneration
{
    internal static class VisualStudioCodePackageGenerator
    {
        internal static void GenerateSnippets(
            SnippetDirectory[] snippetDirectories,
            LanguageDefinition[] languageDefinitions,
            GeneralSettings settings)
        {
            var snippets = new List<Snippet>();

            VisualStudioCodeSnippetGenerator[] generators = languageDefinitions.Select(f => new VisualStudioCodeSnippetGenerator(f)).ToArray();

            foreach (SnippetGeneratorResult result in SnippetGenerator.GetResults(snippetDirectories, generators))
                snippets.AddRange(result.Snippets);

            snippets.AddRange(HtmlSnippetGenerator.GetResult(snippetDirectories).Snippets);
            snippets.AddRange(XmlSnippetGenerator.GetResult(snippetDirectories).Snippets);

            snippets.RemoveAll(f => f.HasTag(KnownTags.ExcludeFromVisualStudioCode));

            foreach (Snippet snippet in snippets)
                snippet.RemoveMetaKeywords();

            foreach (IGrouping<Language, Snippet> grouping in snippets.GroupBy(f => f.Language))
            {
                Console.WriteLine($"{grouping.Key}: {grouping.Count()}");

                var json = new JObject(grouping.Select(ToJson));

                string fileName = Path.ChangeExtension(GetLanguageIdentifier(grouping.Key), "json");

                string filePath = Path.Combine(settings.VisualStudioCodeProjectPath, "json", fileName);

                IOUtility.SaveSnippets(grouping.ToArray(), Path.Combine(settings.VisualStudioCodeProjectPath, $"Snippetica.{grouping.Key}"));

                IOUtility.WriteAllText(filePath, json.ToString(Formatting.Indented));
            }
        }

        private static JProperty ToJson(Snippet snippet)
        {
            return new JProperty(
                snippet.Title,
                new JObject(
                    new JProperty("prefix", snippet.Shortcut),
                    new JProperty("body", new JArray(ConvertToTextMate(snippet).ToLines())),
                    new JProperty("description", snippet.Description)
                )
            );
        }

        private static string GetLanguageIdentifier(Language language)
        {
            switch (language)
            {
                case Language.VisualBasic:
                    return "vb";
                case Language.CSharp:
                    return "csharp";
                case Language.CPlusPlus:
                    return "cpp";
                case Language.Xml:
                    return "xml";
                case Language.JavaScript:
                    return "javascript";
                case Language.Sql:
                    return "sql";
                case Language.Html:
                    return "html";
                case Language.Css:
                    return "css";
                default:
                    throw new InvalidOperationException(language.ToString());
            }
        }

        private static string ConvertToTextMate(Snippet snippet)
        {
            LiteralCollection literals = snippet.Literals;

            string s = snippet.CodeText;

            var sb = new StringBuilder(s.Length);

            int pos = 0;

            PlaceholderCollection placeholders = snippet.Code.Placeholders;

            Dictionary<Literal, int> literalIndexes = literals
                .OrderBy(f => FindMinIndex(f, placeholders))
                .Select((literal, i) => new { Literal = literal, Index = i })
                .ToDictionary(f => f.Literal, f => f.Index + 1);

            var processedIds = new List<string>();

            foreach (Placeholder placeholder in placeholders.OrderBy(f => f.Index))
            {
                sb.Append(s, pos, placeholder.Index - 1 - pos);

                if (placeholder.IsEndPlaceholder)
                {
                    sb.Append("${0}");
                }
                else if (placeholder.IsSelectedPlaceholder)
                {
                    sb.Append("${TM_SELECTED_TEXT}");
                }
                else
                {
                    string id = placeholder.Identifier;

                    Literal literal = literals[id];

                    sb.Append("${");
                    sb.Append(literalIndexes[literal]);

                    if (!processedIds.Contains(id))
                    {
                        sb.Append(":");
                        sb.Append(literal.DefaultValue);
                        processedIds.Add(id);
                    }

                    sb.Append("}");
                }

                pos = placeholder.EndIndex + 1;
            }

            sb.Append(s, pos, s.Length - pos);

            return sb.ToString();
        }

        private static int FindMinIndex(Literal literal, PlaceholderCollection placeholders)
        {
            return placeholders
                .Where(placeholder => placeholder.Identifier == literal.Identifier)
                .Select(placeholder => placeholder.Index)
                .Min();
        }

        private static IEnumerable<string> ToLines(this string value)
        {
            using (var sr = new StringReader(value))
            {
                string line = null;

                while ((line = sr.ReadLine()) != null)
                    yield return line;
            }
        }
    }
}
