using System;

namespace RDS.TextParser.Types
{
    public class SplitInfo
    {
        public string[] Split { get; set; }
        public string Indices { get; set; }
        public string Delimiter { get; set; }
        public bool Exclusion { get; set; }

        public SplitInfo()
        {
            Split = Array.Empty<string>();
            Indices = string.Empty;
            Delimiter = string.Empty;
            Exclusion = false;
        }

        public SplitInfo(string[] split, string indices, string delimiter)
        {
            Split = split;
            Indices = indices;
            Delimiter = delimiter;
            Exclusion = false;
        }

        public SplitInfo(string[] split, string indices, string delimiter, bool exclusion)
        {
            Split = split;
            Indices = indices;
            Delimiter = delimiter;
            Exclusion = exclusion;
        }
    }
}
