﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.Bot.Builder.Expressions;
using Microsoft.Bot.Builder.Expressions.Parser;

namespace Microsoft.Bot.Builder.LanguageGeneration
{
    public class AnalyzerResult
    {
        public AnalyzerResult(List<string> variables = null, List<string> templateReferences = null)
        {
            this.Variables = (variables ?? new List<string>()).Distinct().ToList();
            this.TemplateReferences = (templateReferences ?? new List<string>()).Distinct().ToList();
        }

        public List<string> Variables { get; set; }

        public List<string> TemplateReferences { get; set; }

        public AnalyzerResult Union(AnalyzerResult outputItem)
        {
            this.Variables = this.Variables.Union(outputItem.Variables).ToList();
            this.TemplateReferences = this.TemplateReferences.Union(outputItem.TemplateReferences).ToList();
            return this;
        }
    }
}
