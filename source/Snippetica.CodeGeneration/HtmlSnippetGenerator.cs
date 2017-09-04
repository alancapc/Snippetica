// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Pihrtsoft.Snippets;
using Snippetica.CodeGeneration.Commands;

namespace Snippetica.CodeGeneration
{
    public class HtmlSnippetGenerator : SnippetGenerator
    {
        public override IEnumerable<Snippet> GenerateSnippets(string sourceDirectoryPath, SearchOption searchOption = SearchOption.AllDirectories)
        {
            return base.GenerateSnippets(sourceDirectoryPath, searchOption);

            //TODO: 
            //.Concat(XmlSnippetGenerator.GenerateSnippets(Language.Html));
        }

        protected override JobCollection CreateJobs(Snippet snippet)
        {
            var jobs = new JobCollection();

            if (snippet.Literals.Find("content") != null)
            {
                jobs.Add(new Job(new HtmlWithContentCommand()));
                jobs.Add(new Job(new HtmlWithoutContentCommand()));
            }
            else
            {
                jobs.Add(new Job());
            }

            return jobs;
        }
    }
}
