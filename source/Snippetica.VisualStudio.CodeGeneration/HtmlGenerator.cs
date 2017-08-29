// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using System.Xml;

namespace Snippetica.CodeGeneration.VisualStudio
{
    public static class HtmlGenerator
    {
        public static string GenerateVisualStudioGalleryDescription(SnippetDirectory[] snippetDirectories, GeneralSettings settings)
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
                    x.WriteElementString("h3", settings.ProjectTitle);
                    x.WriteElementString("p", settings.GetProjectSubtitle(snippetDirectories));

                    x.WriteElementString("h3", "Links");

                    x.WriteStartElement("ul");

                    x.WriteStartElement("li");
                    x.WriteStartElement("a");
                    x.WriteAttributeString("href", settings.GitHubPath);
                    x.WriteString("Project Website");
                    x.WriteEndElement();
                    x.WriteEndElement();

                    x.WriteStartElement("li");
                    x.WriteStartElement("a");
                    x.WriteAttributeString("href", $"{settings.GitHubMasterPath}/{settings.ChangeLogFileName}");
                    x.WriteString("Release Notes");
                    x.WriteEndElement();
                    x.WriteEndElement();

                    x.WriteStartElement("li");
                    x.WriteStartElement("a");
                    x.WriteAttributeString("href", "http://pihrt.net/Snippetica/Snippets");
                    x.WriteString("Browse and Search All Snippets");
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
                        x.WriteAttributeString("href", $"{settings.GitHubSourcePath}/{settings.ExtensionProjectName}/{directoryName}/README.md");
                        x.WriteString(directoryName);
                        x.WriteEndElement();
                        x.WriteString($" ({snippetDirectory.EnumerateSnippets().Count()} snippets)");

                        x.WriteString(" (");
                        x.WriteStartElement("a");
                        x.WriteAttributeString("href", $"http://pihrt.net/Snippetica/Snippets?Language={snippetDirectory.Language}");
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
