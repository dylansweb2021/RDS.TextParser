using System.Collections.Generic;

namespace RDS.TextParser.Interfaces
{
    public interface IRDSTextParser
    {
        Dictionary<string, object> GetResultDictionary(string tokens, string source);
    }
}
