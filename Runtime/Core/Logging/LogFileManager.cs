using System.IO;
using System.Text;
using JahroConsole.Core.Context;

namespace JahroConsole.Core.Logging
{
    internal static class LogFileManager
    {

        internal static void Clear()
        {
            if (File.Exists(JahroConfig.LogDetailsFilePath))
            {
                File.Delete(JahroConfig.LogDetailsFilePath);
            }
        }

        internal static long SaveDetailsMessage(string message)
        {
            long position;
            using (StreamWriter writer = new StreamWriter(JahroConfig.LogDetailsFilePath, true))
            {
                position = writer.BaseStream.Position;
                writer.WriteLine(message);
                writer.WriteLine("J#DE");
            }
            return position;
        }

        internal static string ReadDetailsMessage(long position)
        {
            using (StreamReader reader = new StreamReader(JahroConfig.LogDetailsFilePath))
            {
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
                StringBuilder logEntry = new StringBuilder();
                string line;
                while ((line = reader.ReadLine()) != "J#DE")
                {
                    logEntry.AppendLine(line);
                }
                return logEntry.ToString();
            }
        }
    }
}