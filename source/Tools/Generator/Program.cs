// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Snippetica.CodeGeneration.Markdown;
using Snippetica.CodeGeneration.VisualStudio;
using Snippetica.CodeGeneration.VisualStudioCode;
using Snippetica.Validations;

namespace Snippetica.CodeGeneration
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var settings = new GeneralSettings() { SolutionDirectoryPath = @"..\..\..\..\.." };

            SnippetDirectory[] snippetDirectories = SnippetDirectory.LoadFromFile(@"..\..\SnippetDirectories.xml").ToArray();

            CharacterSequence[] characterSequences = CharacterSequence.LoadFromFile(@"..\..\CharacterSequences.xml").ToArray();

            LanguageDefinition[] languageDefinitions = LanguageDefinition.LoadFromFile(@"..\..\LanguageDefinitions.xml").ToArray();

            CharacterSequence.SerializeToXml(Path.Combine(settings.ExtensionProjectPath, "CharacterSequences.xml"), characterSequences);

            SnippetDirectory[] nonDevDirectories = snippetDirectories.Where(f => !f.IsDev).ToArray();
            SnippetDirectory[] devDirectories = snippetDirectories.Where(f => f.IsDev).ToArray();

            foreach (SnippetGeneratorResult result in SnippetGenerator.GetResults(devDirectories, languageDefinitions.Select(f => new VisualStudioSnippetGenerator(f)).ToArray()))
                result.Save();

            foreach (SnippetGeneratorResult result in SnippetGenerator.GetResults(nonDevDirectories, languageDefinitions.Select(f => new VisualStudioSnippetGenerator(f)).ToArray()))
                result.Save();

            HtmlSnippetGenerator.GetResult(snippetDirectories).Save();
            XamlSnippetGenerator.GetResult(snippetDirectories).Save();
            XmlSnippetGenerator.GetResult(snippetDirectories).Save();

            VisualStudioCodeSnippetGenerator.GenerateSnippets(nonDevDirectories, languageDefinitions, characterSequences, settings.VisualStudioCodeProjectPath);
            VisualStudioCodeSnippetGenerator.GenerateSnippets(devDirectories, languageDefinitions, null, settings.VisualStudioCodeProjectPath + ".Dev");

            SnippetDirectory[] releaseDirectories = snippetDirectories.Where(f => f.IsRelease && !f.IsDev).ToArray();

            MarkdownWriter.WriteSolutionReadMe(releaseDirectories, settings);

            MarkdownWriter.WriteProjectReadMe(releaseDirectories, Path.GetFullPath(settings.ProjectPath));

            MarkdownWriter.WriteDirectoryReadMe(
                snippetDirectories
                    .Where(f => f.HasAnyTag(KnownTags.Release, KnownTags.Dev) && !f.IsAutoGeneration)
                    .ToArray(),
                characterSequences,
                new SnippetListSettings("VisualStudio"));

            VisualStudioPackageGenerator.GenerateVisualStudioPackageFiles(
                directories: releaseDirectories,
                characterSequences: characterSequences,
                settings: settings);

            settings.ExtensionProjectName += ".Dev";

            VisualStudioPackageGenerator.GenerateVisualStudioPackageFiles(
                directories: snippetDirectories
                    .Where(f => f.HasTags(KnownTags.Release, KnownTags.Dev))
                    .ToArray(),
                characterSequences: null,
                settings: settings);

            SnippetChecker.CheckSnippets(snippetDirectories);

            Console.WriteLine("*** END ***");
            Console.ReadKey();
        }
    }
}
