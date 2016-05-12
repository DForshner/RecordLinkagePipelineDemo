using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Processor.Log
{
    /// <summary>
    /// Normally you would use an off the shelf async thread safe logger but I'm trying to minimise the number of libraries used.
    /// </summary>
    public class Logger
    {
        private BlockingCollection<string> _linesToLog = new BlockingCollection<string>(new ConcurrentQueue<string>());

        private readonly Task _writeToLoggingFile;

        public Logger(string fileName)
        {
            _writeToLoggingFile = Task.Run(() => WriteLogFile(fileName));
        }

        /// <summary>
        /// Consume log messages from thread safe queue and write to the log file.
        /// </summary>
        private void WriteLogFile(string filename)
        {
            using (var file = new System.IO.StreamWriter(filename))
            {
                foreach (var msg in _linesToLog.GetConsumingEnumerable())
                {
                    file.WriteLine(msg);
                }
            }
        }

        /// <summary>
        /// Write log messages to a thread safe queue that will be consumed in another thread.
        /// </summary>
        public void Log(string msg)
        {
            _linesToLog.Add(msg);
        }

        /// <summary>
        /// Stop logging and wait for log queue to empty.
        /// </summary>
        public void StopLogging()
        {
            _linesToLog.CompleteAdding();
            _writeToLoggingFile.Wait();
        }
    }
}