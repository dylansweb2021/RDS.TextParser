using System;

namespace RDS.TextParser.Interfaces
{
    public interface ISourceClient
    {
        string GetSourceAsString(Uri uri);
    }
}
