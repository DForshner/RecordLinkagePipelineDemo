using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Pipeline.Shared;

namespace Pipeline.Extraction
{
    public class IngestListingFile
    {
        public IEnumerable<Listing> Ingest(string fileName, int? limit = null)
        {
            int count = 0;
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    while (reader.Peek() >= 0)
                    {
                        var line = reader.ReadLine();
                        Listing ret = null;
                        try
                        {
                            count++;
                            ret = ListingParser.Parse(line);
                        }
                        catch
                        {
                            // TODO: Log Parse error
                            continue;
                        }

                        if (limit.HasValue && count >= limit.Value) { break; }

                        yield return ret;
                    }
                }
            }
        }
    }
}