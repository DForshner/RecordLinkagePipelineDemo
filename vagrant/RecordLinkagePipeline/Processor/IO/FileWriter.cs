using System.Collections.Generic;
using Pipeline.Shared;

namespace Processor.IO
{
    public class FileWriter
    {
        public void WriteLines(string fileName, IEnumerable<string> lines)
        {
            System.IO.File.WriteAllLines(fileName, lines);
        }
    }
}