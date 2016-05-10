using System.Collections.Generic;

namespace Pipeline.Shared
{
    public interface ITextReader
    {
        IEnumerable<string> ReadLines(string fileName, int? limit = null);
    }
}
