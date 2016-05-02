using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Pipeline.Shared;

namespace Pipeline.Extraction
{
    public class IngestProductFile
    {
        public IEnumerable<Product> Ingest(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    while (reader.Peek() >= 0)
                    {
                        var line = reader.ReadLine();
                        Product ret = null;
                        try
                        {
                            ret = ProductParser.Parse(line);
                        }
                        catch
                        {
                            // TODO: Log Parse error
                            continue;
                        }

                        yield return ret;
                    }
                }
            }
        }
    }
}