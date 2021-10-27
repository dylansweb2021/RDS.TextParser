namespace RDS.TextParser.Tokens
{
    public class JSONToken : Token
    {
        public string Key { get; set; }

        public JSONToken()
        {
            Key = string.Empty;
        }

        public JSONToken(Token token)
        {
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

            Key = string.Empty;
        }
    }
}
