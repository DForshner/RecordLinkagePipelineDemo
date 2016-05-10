using System.Collections.Generic;

namespace Pipeline.Shared
{
    public interface ITextWriter
    {
        void WriteLines(string fileName, IEnumerable<string> lines);
    }
}
