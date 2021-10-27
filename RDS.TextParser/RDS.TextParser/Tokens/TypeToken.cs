using System.Collections.Generic;

namespace RDS.TextParser.Tokens
{
    public class Type_Token : Token
    {
        public List<Type_Token> Properties;

        public Type_Token()
        {
            Properties = new List<Type_Token>();
        }

        public Type_Token(Token txt)
        {
            Append = txt.Append;
            Delimiter = txt.Delimiter;
            Name = txt.Name;
            Prepend = txt.Prepend;
            Replacements = txt.Replacements;
            Sections = txt.Sections;
            Splits = txt.Splits;
            SplitIndices = txt.SplitIndices;
            Trim = txt.Trim;
            TrimZone = txt.TrimZone;
            TrimTags = txt.TrimTags;
            TrimTagCount = txt.TrimTagCount;
            ValidationExpression = txt.ValidationExpression;
            DecodeHTML = txt.DecodeHTML;
            DecodeUrl = txt.DecodeUrl;
            Extractions = txt.Extractions;
            objJSON = txt.objJSON;
            Operator = txt.Operator;
            Type = txt.Type;
            Validations = txt.Validations;
            Properties = new List<Type_Token>();
        }
    }
}
