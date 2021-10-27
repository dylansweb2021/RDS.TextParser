using System;
using System.Collections.Generic;

namespace RDS.TextParser.Tokens
{
    public class ConditionalToken : Token
    {
        public List<Token> Conditions { get; set; }
        public Token ResultTrue { get; set; }
        public Token ResultFalse { get; set; }

        public ConditionalToken()
        {
            Conditions = new List<Token>(2);
        }

        public ConditionalToken(Token token)
        {
            Conditions = new List<Token>(2);
            Append = token.Append;
            Delimiter = token.Delimiter;
            Name = token.Name;
            Prepend = token.Prepend;
            Replacements = token.Replacements;
            Sections = token.Sections;
            Splits = token.Splits;
            SplitIndices = token.SplitIndices;
            Trim = token.Trim;
            TrimZone = token.TrimZone;
            ValidationExpression = token.ValidationExpression;
        }
    }
}
