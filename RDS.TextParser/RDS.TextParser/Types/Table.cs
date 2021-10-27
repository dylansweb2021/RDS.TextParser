using System.Collections.Generic;

namespace RDS.TextParser.Types
{
    public class Table
    {
        public string Title { get; set; }

        public List<string> Headers { get; set; }

        public List<string[]> Rows { get; set; }

        public Table()
        {
            Title = string.Empty;
            Headers = new List<string>(0);
            Rows = new List<string[]>(0);
        }

        public string[] GetColumn(int index)
        {
            List<string> rows = new List<string>(Rows.Count);

            if (Rows.Count == 0 || index < 0 || index >= Rows[0].Length) { return rows.ToArray(); }

            for (int i = 0; i < Rows.Count; i++)
            {
                if (index >= Rows[i].Length) { continue; }

                rows.Add(Rows[i][index]);
            }

            return rows.ToArray();
        }
    }
}
