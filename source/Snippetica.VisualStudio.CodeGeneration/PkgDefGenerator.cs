// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Linq;
using Pihrtsoft.Snippets;

namespace Snippetica.CodeGeneration.VisualStudio
{
    public static class PkgDefGenerator
    {
        public static string GeneratePkgDefFile(SnippetDirectory[] snippetDirectories)
        {
            using (var sw = new StringWriter())
            {
                foreach (IGrouping<Language, SnippetDirectory> grouping in snippetDirectories.GroupBy(f => f.Language))
                {
                    sw.WriteLine($"// {grouping.Key.GetTitle()}");

                    foreach (SnippetDirectory snippetDirectory in grouping)
                    {
                        sw.WriteLine($@"[$RootKey$\Languages\CodeExpansions\{snippetDirectory.Language.GetRegistryCode()}\Paths]");
                        sw.WriteLine($"\"{snippetDirectory.DirectoryName}\" = \"$PackageFolder$\\{snippetDirectory.DirectoryName}\"");
                    }

                    sw.WriteLine();
                }

                return sw.ToString();
            }
        }
    }
}
