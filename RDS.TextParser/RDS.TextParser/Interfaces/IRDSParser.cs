using System.Collections.Generic;

namespace RDS.TextParser.Interfaces
{
    public interface IRDSParser
    {
        Dictionary<string, object> GetResultDictionary(string tokens, string source);
    }
}
