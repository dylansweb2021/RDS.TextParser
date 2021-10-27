using System;

namespace RDS.TextParser.Types
{
    public class Section
    {
        public string[] Begin { get; set; }
        public string[] End { get; set; }
        public int[] IndicesBegin { get; set; }
        public int[] IndicesEnd { get; set; }
        public Section AlternateSection { get; set; }
        public bool BeginIsExp { get; set; }
        public bool EndIsExp { get; set; }
        public bool BeginIsCaseSensitive { get; set; } = true;
        public bool EndIsCaseSensitive { get; set; } = true;
        public Tag Tag { get; set; }

        public Section()
        {
            AlternateSection = null;
            IndicesBegin = Array.Empty<int>();
            IndicesEnd = Array.Empty<int>();
        }

        public Section(string begin, string end, int[] indicesBegin, int[] indicesEnd, bool beginIsCaseSensitive, bool endIsCaseSensitive)
        {
            Begin = new string[] { begin };
            End = new string[] { end };
            IndicesBegin = indicesBegin ?? Array.Empty<int>();
            IndicesEnd = indicesEnd ?? Array.Empty<int>();
            BeginIsCaseSensitive = beginIsCaseSensitive;
            EndIsCaseSensitive = endIsCaseSensitive;
            BeginIsExp = false;
            EndIsExp = false;
            AlternateSection = null;
            Tag = null;
        }

        public Section(string begin, bool beginIsExpr, string end, bool endIsExpr, int[] indicesBegin, int[] indicesEnd)
        {
            Begin = new string[] { begin };
            End = new string[] { end };
            IndicesBegin = indicesBegin ?? Array.Empty<int>();
            IndicesEnd = indicesEnd ?? Array.Empty<int>();
            BeginIsExp = beginIsExpr;
            EndIsExp = endIsExpr;
            AlternateSection = null;
            Tag = null;
        }
    }
}
