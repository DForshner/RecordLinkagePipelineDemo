using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Processor.IO
{
    public class FileReader
    {
        public IEnumerable<string> ReadLines(string fileName, int? limit = null)
        {
            int count = 0;
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    while (reader.Peek() >= 0 && (!limit.HasValue || count < limit.Value))
                    {
                       count++;
                       yield return reader.ReadLine();
                    }
                }
            }
        }
    }
}