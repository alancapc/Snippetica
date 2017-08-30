// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Pihrtsoft.Records;
using Pihrtsoft.Snippets;

namespace Snippetica
{
    [DebuggerDisplay("{Language} Tags = {TagsText,nq} Path = {Path,nq}")]
    public class SnippetDirectory
    {
        private static readonly ReadOnlyCollection<string> _noTags = new ReadOnlyCollection<string>(new string[0]);

        public SnippetDirectory(string path, Language language, params string[] tags)
        {
            Path = path;
            Language = language;

            if (tags?.Length > 0)
            {
                Tags = new ReadOnlyCollection<string>(tags);
            }
            else
            {
                Tags = _noTags;
            }
        }

        public string Path { get; }

        public Language Language { get; }

        public ReadOnlyCollection<string> Tags { get; }

        public string DirectoryName
        {
            get { return System.IO.Path.GetFileName(Path); }
        }

        public string LanguageTitle
        {
            get { return LanguageHelper.GetLanguageTitle(Language); }
        }

        public SnippetDirectory WithPath(string path)
        {
            return new SnippetDirectory(path, Language, Tags.ToArray());
        }

        public bool HasTag(string tag)
        {
            return Tags.Any(f => f.Equals(tag, StringComparison.Ordinal));
        }

        public bool HasTags(params string[] tags)
        {
            foreach (string tag in tags)
            {
                if (!HasTag(tag))
                    return false;
            }

            return true;
        }

        public bool HasAnyTag(params string[] tags)
        {
            foreach (string tag in tags)
            {
                if (HasTag(tag))
                    return true;
            }

            return false;
        }

        private string TagsText
        {
            get { return string.Join(", ", Tags); }
        }

        public static IEnumerable<SnippetDirectory> LoadFromFile(string url)
        {
            return Document.ReadRecords(url)
                .Where(f => !f.HasTag(KnownTags.Disabled))
                .Select(SnippetDirectoryMapper.MapFromRecord);
        }

        public IEnumerable<Snippet> EnumerateSnippets(SearchOption searchOption = SearchOption.AllDirectories)
        {
            return SnippetSerializer.Deserialize(Path, searchOption);
        }

        public bool IsRelease
        {
            get { return HasTag(KnownTags.Release); }
        }

        public bool IsDev
        {
            get { return HasTag(KnownTags.Dev); }
        }

        public bool IsAutoGeneration
        {
            get { return HasAnyTag(KnownTags.AutoGenerationSource, KnownTags.AutoGenerationDestination); }
        }

        public bool IsAutoGenerationSource
        {
            get { return HasTag(KnownTags.AutoGenerationSource); }
        }

        public bool IsAutoGenerationDestination
        {
            get { return HasTag(KnownTags.AutoGenerationDestination); }
        }
    }
}
