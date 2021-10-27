namespace RDS.TextParser.Tokens
{
    public class TypeListToken : ListToken
    {
        public Type_Token TypeToken;

        public TypeListToken()
        {
            TypeToken = new Type_Token();
        }

        public TypeListToken(ListToken token)
        {
            Append = token.Append;
            DecodeHTML = token.DecodeHTML;
            Delimiter = token.Delimiter;
            Element = token.Element;
            Name = token.Name;
            Operator = token.Operator;
            Prepend = token.Prepend;
            Replacements = token.Replacements;
            Sections = token.Sections;
            Splits = token.Splits;
            SplitIndices = token.SplitIndices;
            Trim = token.Trim;
            TrimZone = token.TrimZone;
            Type = token.Type;
            ValidationExpression = token.ValidationExpression;

            TypeToken = new Type_Token();
        }

    }
}
