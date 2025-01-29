using System.IO;
using System.Text;
using UnityEngine;

namespace JahroConsole.Core.Logging
{
    internal static class LogFileManager
    {

        internal static void Clear()
        {
            if (File.Exists(GetLocalSavesForDetails()))
            {
                File.Delete(GetLocalSavesForDetails());
            }
        }

        internal static long SaveDetailsMessage(string message)
        {
            long position;
            using (StreamWriter writer = new StreamWriter(GetLocalSavesForDetails(), true))
            {
                position = writer.BaseStream.Position;
                writer.WriteLine(message);
                writer.WriteLine("J#DE");
            }
            return position;
        }

        internal static string ReadDetailsMessage(long position)
        {
            using (StreamReader reader = new StreamReader(GetLocalSavesForDetails()))
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

        private static string GetLocalSavesForDetails()
        {
            string folderPath = Application.persistentDataPath;
            string filename = "jahro-log-details.dat";
            return folderPath + Path.DirectorySeparatorChar + filename;
        }
    }
}