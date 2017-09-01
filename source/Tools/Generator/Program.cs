// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Snippetica.CodeGeneration.Markdown;
using Snippetica.CodeGeneration.VisualStudio;
using Snippetica.CodeGeneration.VisualStudioCode;
using Snippetica.Validations;
using static Snippetica.KnownNames;
using static Snippetica.KnownPaths;

namespace Snippetica.CodeGeneration
{
    internal static class Program
    {
        private const string DevelopmentSuffix = ".Dev";

        private static void Main(string[] args)
        {
            SnippetDirectory[] snippetDirectories = SnippetDirectory.LoadFromFile(@"..\..\SnippetDirectories.xml").ToArray();

            CharacterSequence[] characterSequences = CharacterSequence.LoadFromFile(@"..\..\CharacterSequences.xml").ToArray();

            LanguageDefinition[] languageDefinitions = LanguageDefinition.LoadFromFile(@"..\..\LanguageDefinitions.xml").ToArray();

            CharacterSequence.SerializeToXml(Path.Combine(VisualStudioExtensionProjectPath, "CharacterSequences.xml"), characterSequences);

            SnippetDirectory[] nonDevDirectories = snippetDirectories.Where(f => !f.IsDev).ToArray();
            SnippetDirectory[] devDirectories = snippetDirectories.Where(f => f.IsDev).ToArray();

            foreach (SnippetGeneratorResult result in SnippetGenerator.GetResults(devDirectories, languageDefinitions.Select(f => new VisualStudioSnippetGenerator(f)).ToArray()))
                result.Save();

            foreach (SnippetGeneratorResult result in SnippetGenerator.GetResults(nonDevDirectories, languageDefinitions.Select(f => new VisualStudioSnippetGenerator(f)).ToArray()))
                result.Save();

            HtmlSnippetGenerator.GetResult(snippetDirectories).Save();
            XamlSnippetGenerator.GetResult(snippetDirectories).Save();
            XmlSnippetGenerator.GetResult(snippetDirectories).Save();

            VisualStudioCodeSnippetGenerator.GenerateSnippets(nonDevDirectories, languageDefinitions, characterSequences.Where(f => !f.HasTag(KnownTags.ExcludeFromVisualStudioCode)).ToArray(), VisualStudioCodeExtensionProjectPath);
            VisualStudioCodeSnippetGenerator.GenerateSnippets(devDirectories, languageDefinitions, null, VisualStudioCodeExtensionProjectPath + DevelopmentSuffix);

            SnippetDirectory[] releaseDirectories = snippetDirectories.Where(f => f.IsRelease && !f.IsDev).ToArray();

            MarkdownWriter.WriteSolutionReadMe(releaseDirectories);

            MarkdownWriter.WriteProjectReadMe(releaseDirectories, Path.Combine(SolutionDirectoryPath, SourceDirectoryName, ProductName));

            MarkdownWriter.WriteDirectoryReadMe(
                snippetDirectories
                    .Where(f => f.HasAnyTag(KnownTags.Release, KnownTags.Dev) && !f.IsAutoGeneration)
                    .ToArray(),
                characterSequences,
                SnippetListSettings.VisualStudio);

            VisualStudioPackageGenerator.GenerateVisualStudioPackageFiles(
                VisualStudioExtensionProjectPath,
                directories: releaseDirectories,
                characterSequences: characterSequences);

            VisualStudioPackageGenerator.GenerateVisualStudioPackageFiles(
                VisualStudioExtensionProjectPath + DevelopmentSuffix,
                directories: snippetDirectories.Where(f => f.HasTags(KnownTags.Release, KnownTags.Dev)).ToArray(),
                characterSequences: null);

            SnippetChecker.CheckSnippets(snippetDirectories);

            Console.WriteLine("*** END ***");
            Console.ReadKey();
        }
    }
}
