namespace RDS.TextParser.Types
{
    public class Replacement
    {
        public string OldText { get; set; } = string.Empty;

        public string NewText { get; set; } = string.Empty;

        public Replacement(string oldText)
        {
            OldText = oldText;
        }

        public Replacement(string oldText, string newText)
        {
            OldText = oldText;
            NewText = newText;
        }
    }
}
