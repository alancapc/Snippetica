// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pihrtsoft.Records;
using Pihrtsoft.Snippets;
using Snippetica.CodeGeneration.Markdown;
using Snippetica.CodeGeneration.VisualStudio;
using static Snippetica.KnownPaths;

namespace Snippetica.CodeGeneration
{
    internal static class Program
    {
        private const string DevelopmentSuffix = ".Dev";

        private static void Main(string[] args)
        {
            SnippetDirectory[] snippetDirectories = LoadSnippetDirectories(@"..\..\SnippetDirectories.xml").ToArray();

            CharacterSequence[] characterSequences = CharacterSequence.LoadFromFile(@"..\..\CharacterSequences.xml").ToArray();

            LoadLanguageDefinitions();

            VisualStudioPackageGenerator.GeneratePackageFiles(
                snippetDirectories,
                VisualStudioExtensionProjectPath,
                characterSequences);

            VisualStudioCodePackageGenerator.GeneratePackageFiles(
                snippetDirectories,
                VisualStudioCodeExtensionProjectPath,
                characterSequences);

            //TODO: WriteSolutionReadme

            CharacterSequence.SerializeToXml(Path.Combine(VisualStudioExtensionProjectPath, "CharacterSequences.xml"), characterSequences);

            Console.WriteLine("*** END ***");
            Console.ReadKey();
        }

        public static IEnumerable<SnippetDirectory> LoadSnippetDirectories(string url)
        {
            return Document.ReadRecords(url)
                .Where(f => !f.HasTag(KnownTags.Disabled))
                .Select(SnippetDirectoryMapper.MapFromRecord);
        }

        public static void LoadLanguageDefinitions()
        {
            LanguageDefinition[] languageDefinitions = Document.ReadRecords(@"..\..\LanguageDefinitions.xml")
                .Where(f => !f.HasTag(KnownTags.Disabled))
                .ToLanguageDefinitions()
                .ToArray();

            LanguageDefinition.CSharp = languageDefinitions.First(f => f.Language == Language.CSharp);
            LanguageDefinition.VisualBasic = languageDefinitions.First(f => f.Language == Language.VisualBasic);
        }
    }
}
