// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Snippets;

namespace Snippetica.CodeGeneration
{
    public abstract class SnippetEnvironment
    {
        public IEnumerable<SnippetGeneratorResult> GenerateSnippets(IEnumerable<SnippetDirectory> snippetDirectories)
        {
            foreach (SnippetDirectory snippetDirectory in snippetDirectories)
            {
                foreach (SnippetGeneratorResult result in GenerateSnippets(snippetDirectory))
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

                yield return new SnippetGeneratorResult(GenerateSnippetsCore(devDirectory), devDirectory.WithPath(directory.Path));
            }
        }

        private List<Snippet> GenerateSnippetsCore(SnippetDirectory directory)
        {
            var snippets = new List<Snippet>();

            snippets.AddRange(directory.EnumerateSnippets(SearchOption.TopDirectoryOnly));

            string autoGenerationPath = Path.Combine(directory.Path, "AutoGeneration");

            //TODO: 
            snippets.AddRange(SnippetGenerator.GenerateAlternativeShortcuts(snippets));

            if (Directory.Exists(autoGenerationPath))
            {
                SnippetDirectory autoGenerationDirectory = directory.WithPath(autoGenerationPath);

                SnippetGenerator generator = CreateGenerator(autoGenerationDirectory);

                snippets.AddRange(generator.GenerateSnippets(autoGenerationDirectory.Path));
            }

            return snippets;
        }

        public virtual bool ShouldGenerateSnippets(SnippetDirectory directory)
        {
            return IsSupportedLanguage(directory.Language);
        }

        public abstract SnippetGenerator CreateGenerator(SnippetDirectory directory);

        public abstract bool IsSupportedLanguage(Language language);
    }
}
