// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets;

namespace Snippetica.CodeGeneration
{
    public abstract class SnippetEnvironment
    {
        public abstract Engine Engine { get; }

        public IEnumerable<SnippetGeneratorResult> GenerateSnippets(IEnumerable<SnippetDirectory> directories)
        {
            foreach (SnippetDirectory directory in directories)
            {
                foreach (SnippetGeneratorResult result in GenerateSnippets(directory))
                {
                    yield return result;
                }
            }
        }

        public IEnumerable<SnippetGeneratorResult> GenerateSnippets(SnippetDirectory directory)
        {
            if (!ShouldGenerateSnippets(directory))
                yield break;

            yield return new SnippetGeneratorResult(GenerateSnippetsCore(directory), directory);

            string devPath = Path.Combine(directory.Path, "Dev");

            if (Directory.Exists(devPath))
            {
                //TODO: 
                List<string> tags = directory.Tags.ToList();

                tags.Add(KnownTags.Dev);

                var devDirectory = new SnippetDirectory(devPath, directory.Language, tags.ToArray());

                yield return new SnippetGeneratorResult(GenerateSnippetsCore(devDirectory), devDirectory.WithPath(directory.Path + KnownNames.DevSuffix));
            }
        }

        private List<Snippet> GenerateSnippetsCore(SnippetDirectory directory)
        {
            var snippets = new List<Snippet>();

            snippets.AddRange(EnumerateSnippets(directory.Path));

            snippets.AddRange(SnippetGenerator.GenerateAlternativeShortcuts(snippets));

            if (!directory.IsDevelopment
                && directory.HasTag(KnownTags.GenerateXmlSnippets))
            {
                switch (directory.Language)
                {
                    case Language.Xml:
                    case Language.Xaml:
                    case Language.Html:
                        {
                            snippets.AddRange(XmlSnippetGenerator.GenerateSnippets(directory.Language));
                            break;
                        }
                }
            }

            string autoGenerationPath = Path.Combine(directory.Path, "AutoGeneration");

            if (Directory.Exists(autoGenerationPath))
            {
                SnippetDirectory autoGenerationDirectory = directory.WithPath(autoGenerationPath);

                SnippetGenerator generator = CreateGenerator(autoGenerationDirectory);

                snippets.AddRange(generator.GenerateSnippets(autoGenerationDirectory.Path));
            }

            return snippets;
        }

        private static IEnumerable<Snippet> EnumerateSnippets(string directoryPath)
        {
            foreach (string path in Directory.EnumerateDirectories(directoryPath, "*", SearchOption.TopDirectoryOnly))
            {
                string name = Path.GetFileName(path);

                if (name == "Dev")
                    continue;

                if (name == "AutoGeneration")
                    continue;

                foreach (Snippet snippet in SnippetSerializer.Deserialize(path, SearchOption.AllDirectories))
                {
                    yield return snippet;
                }
            }

            foreach (string filePath in Directory.EnumerateFiles(directoryPath, SnippetFileSearcher.Pattern, SearchOption.TopDirectoryOnly))
            {
                foreach (Snippet snippet in SnippetSerializer.DeserializeFile(filePath).Snippets)
                {
                    yield return snippet;
                }
            }
        }

        public virtual bool ShouldGenerateSnippets(SnippetDirectory directory)
        {
            return IsSupportedLanguage(directory.Language);
        }

        public abstract SnippetGenerator CreateGenerator(SnippetDirectory directory);

        public abstract bool IsSupportedLanguage(Language language);
    }
}
