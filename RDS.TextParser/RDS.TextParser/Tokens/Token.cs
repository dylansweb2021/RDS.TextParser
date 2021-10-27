using System;
using System.Collections.Generic;
using RDS.TextParser.Types;

namespace RDS.TextParser.Tokens
{
    public enum TokenKind
    {
        Text,
        Type,
        List,
        TypeList,
        Conditional,
        JSON
    }

    public class Token
    {
        public List<Section> Sections { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public List<SplitInfo> Splits { get; set; }
        public string SplitIndices { get; set; }
        public List<Replacement> Replacements { get; set; }
        public char[] Trim { get; set; }
        public string TrimZone { get; set; }
        public bool TrimTags { get; set; }
        public int TrimTagCount { get; set; }
        public string Delimiter { get; set; }
        public string Prepend { get; set; }
        public string Append { get; set; }
        public string ValidationExpression { get; set; }
        public string Operator { get; set; }
        public bool DecodeHTML { get; set; }
        public bool DecodeUrl { get; set; }
        public object objJSON { get; set; }
        public string[] Validations { get; set; }
        public string[] Extractions { get; set; }

        public Token()
        {
            Splits = new List<SplitInfo>(0);
            DecodeHTML = false;
            DecodeUrl = false;
            Type = string.Empty;
            Sections = new List<Section>(2);
            Replacements = new List<Replacement>(2);
            Extractions = Array.Empty<string>();
            Validations = Array.Empty<string>();
            Operator = "AND";
        }

        public Token(string appendText)
        {
            Splits = new List<SplitInfo>(0);
            DecodeHTML = false;
            DecodeUrl = false;
            Type = string.Empty;
            Sections = new List<Section>(2);
            Replacements = new List<Replacement>(2);
            Extractions = Array.Empty<string>();
            Validations = Array.Empty<string>();
            Append = appendText;
            Operator = "AND";
        }
    }
}
