using System;
using System.IO;
using System.Threading.Tasks;

namespace JahroConsole.Core.Snapshots
{
    public static class FileUploadHelpers
    {
        public static string CreateGzipTempFile(string inputFilePath)
        {
            if (!File.Exists(inputFilePath)) throw new FileNotFoundException(inputFilePath);

            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".gz");

            // Buffer size: 16KB 
            const int bufferSize = 16 * 1024;

            using (var source = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, FileOptions.SequentialScan))
            using (var destination = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, bufferSize, FileOptions.SequentialScan))
            using (var gzip = new System.IO.Compression.GZipStream(destination, System.IO.Compression.CompressionLevel.Optimal))
            {
                source.CopyTo(gzip, bufferSize);
                // flush by disposing the GZipStream
            }

            return tempPath;
        }

        public static bool ShouldGzip(string inputFilePath, long thresholdBytes = 16 * 1024)
        {
            try
            {
                var fi = new FileInfo(inputFilePath);
                return fi.Length > thresholdBytes;
            }
            catch { return true; }
        }
    }
}
