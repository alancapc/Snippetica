// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Xml;
using static Snippetica.KnownPaths;
using static Snippetica.KnownNames;

namespace Snippetica.CodeGeneration.Package.VisualStudio
{
    public static class HtmlGenerator
    {
        public static string GenerateVisualStudioMarketplaceDescription(SnippetDirectory[] snippetDirectories)
        {
            using (var sw = new StringWriter())
            {
                var xmlWriterSettings = new XmlWriterSettings()
                {
                    Indent = true,
                    IndentChars = "  ",
                    ConformanceLevel = ConformanceLevel.Fragment
                };

                using (XmlWriter x = XmlWriter.Create(sw, xmlWriterSettings))
                {
                    x.WriteElementString("h3", ProductName);
                    x.WriteElementString("p", CodeGenerationUtility.GetProjectSubtitle(snippetDirectories));

                    x.WriteElementString("h3", "Links");

                    x.WriteStartElement("ul");

                    x.WriteStartElement("li");
                    x.WriteStartElement("a");
                    x.WriteAttributeString("href", GitHubUrl);
                    x.WriteString("Project Website");
                    x.WriteEndElement();
                    x.WriteEndElement();

                    x.WriteStartElement("li");
                    x.WriteStartElement("a");
                    x.WriteAttributeString("href", $"{MasterGitHubUrl}/{ChangeLogFileName}");
                    x.WriteString("Release Notes");
                    x.WriteEndElement();
                    x.WriteEndElement();

                    x.WriteStartElement("li");
                    x.WriteString("Browse all available snippets with ");
                    x.WriteStartElement("a");
                    x.WriteAttributeString("href", GetSnippetBrowserUrl(Engine.VisualStudio));
                    x.WriteString("Snippet Browser");
                    x.WriteEndElement();
                    x.WriteEndElement();

                    x.WriteEndElement();

                    x.WriteElementString("h3", "Snippets");
                    x.WriteStartElement("ul");

                    foreach (SnippetDirectory snippetDirectory in snippetDirectories)
                    {
                        string directoryName = Path.GetFileName(snippetDirectory.Path);

                        x.WriteStartElement("li");

                        x.WriteStartElement("a");
                        x.WriteAttributeString("href", $"{VisualStudioExtensionGitHubUrl}/{directoryName}/{ReadMeFileName}");
                        x.WriteString(directoryName);
                        x.WriteEndElement();
                        x.WriteString($" ({snippetDirectory.EnumerateSnippets().Count()} snippets)");

                        x.WriteString(" (");
                        x.WriteStartElement("a");
                        x.WriteAttributeString("href", GetSnippetBrowserUrl(Engine.VisualStudio, snippetDirectory.Language));
                        x.WriteString("full list");
                        x.WriteEndElement();
                        x.WriteString(")");

                        x.WriteEndElement();
                    }

                    x.WriteEndElement();
                }

                return sw.ToString();
            }
        }
    }
}
