using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Processor.IO
{
    public class FileWriter
    {
        public void WriteLines(string fileName, IEnumerable<string> lines)
        {
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                //using (StreamReader reader = new StreamReader(file))
                //{
                    //while (reader.Peek() >= 0 && (!limit.HasValue || count < limit.Value))
                    //{
                       //yield return reader.ReadLine();
                    //}
                //}
            }
        }
    }
}