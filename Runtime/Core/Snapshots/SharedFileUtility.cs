using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    public static class SharedFileUtility
    {
        public static async Task WriteAtomicFileAsync(string tempPath, string finalPath,
            Func<Stream, Task> writeFunc, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(tempPath) || string.IsNullOrEmpty(finalPath))
                throw new ArgumentException("Paths cannot be null or empty");

            if (writeFunc == null)
                throw new ArgumentNullException(nameof(writeFunc));

            var directory = Path.GetDirectoryName(finalPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write,
                    FileShare.None, JahroConfig.DefaultBufferSize, FileOptions.WriteThrough);

                await writeFunc(fileStream);

                await fileStream.FlushAsync(ct);
                TryFsync(fileStream);

                fileStream.Dispose();
                fileStream = null;

                if (!TryRename(tempPath, finalPath))
                {
                    throw new IOException($"Failed to atomically rename {tempPath} to {finalPath}");
                }
            }
            catch (Exception)
            {
                fileStream?.Dispose();
                DeleteFileSafe(tempPath);
                throw;
            }
        }

        public static void DeleteFileSafe(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to delete file {path}: {ex.Message}");
            }
        }

        public static bool TryRename(string src, string dst)
        {
            try
            {
#if UNITY_WEBGL
                // WebGL: Simple move, atomicity is best-effort
                File.Move(src, dst);
                return true;
#else
                // Desktop platforms: Delete destination first if exists, then move
                if (File.Exists(dst))
                {
                    File.Delete(dst);
                }
                File.Move(src, dst);
                return true;
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Rename failed from {src} to {dst}: {ex.Message}");
                return false;
            }
        }

        public static bool TryFsync(FileStream fs)
        {
            if (fs == null) return false;

            try
            {
#if UNITY_WEBGL
                // WebGL: No fsync support, IndexedDB commits are eventual
                return true; // Return true to indicate "best effort"
#else
                // Desktop platforms: Force OS to write buffers to disk
                fs.Flush(true); // flushToDisk = true
                return true;
#endif
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Fsync failed: {ex.Message}");
                return false;
            }
        }

        public static void RemoveDirectorySafe(string directoryPath)
        {
            if (string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
            {
                return;
            }

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(directoryPath);
                foreach (var fileInfo in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    fileInfo.Attributes &= ~FileAttributes.ReadOnly;
                }

                Directory.Delete(directoryPath, true);
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.LogError($"Access denied when removing directory: {ex.Message}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"IO error when removing directory: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to remove directory: {ex.Message}");
            }
        }

        public static void EnsureDirectoryExists(string directoryPath)
        {
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public static T LoadJsonFromFile<T>(string filePath) where T : class
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                var json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load JSON from {filePath}: {ex.Message}");
                return null;
            }
        }

        public static void SaveJsonToFile<T>(T obj, string filePath) where T : class
        {
            if (obj == null || string.IsNullOrEmpty(filePath))
                return;

            try
            {
                var directory = Path.GetDirectoryName(filePath);
                EnsureDirectoryExists(directory);

                var json = JsonUtility.ToJson(obj, true);
                File.WriteAllText(filePath, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save JSON to {filePath}: {ex.Message}");
            }
        }

        public static string[] GetFilesWithPattern(string directoryPath, string pattern)
        {
            if (!Directory.Exists(directoryPath))
                return new string[0];

            try
            {
                return Directory.GetFiles(directoryPath, pattern);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get files from {directoryPath}: {ex.Message}");
                return new string[0];
            }
        }
    }
}
