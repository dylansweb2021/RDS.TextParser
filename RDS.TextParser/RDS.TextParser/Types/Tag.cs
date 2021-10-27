namespace RDS.TextParser.Types
{
    public class Tag
    {
        public enum Targets
        {
            // what comes immediately after the tag
            After,
            // what comes immediately before the tag
            Before,
            // what comes between the tag and it's closing tag
            Between,
            // the count of matching tags
            Count
        }

        // the name of the tag to parse on
        public string Name { get; set; }

        // any attributes that must be present in tag for it to be a valid selection - default is don't care
        public string[] Attributes { get; set; }

        // specifies whether or not to include the closing tag in the counting of tags from start...default is don't care.
        public bool Closed { get; set; }

        // the index in an array of matching tags to be considered as the 'first' tag - default is zero
        public int FirstTag { get; set; }

        // the index in an array of matching tags to be considered as the 'last' tag - default is last available.
        public int LastTag { get; set; }

        // inidcates if target is what comes before, between or after the target tag
        public Targets Target { get; set; }
    }
}
