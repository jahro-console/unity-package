using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    public static class ChunkFileUtility
    {
        public static List<ChunkMeta> LoadExistingChunks(string metaPath, string sessionId)
        {
            var chunks = new List<ChunkMeta>();

            try
            {
                if (!Directory.Exists(metaPath)) return chunks;

                var metaFiles = SharedFileUtility.GetFilesWithPattern(metaPath, "*.meta");
                foreach (var metaFile in metaFiles)
                {
                    try
                    {
                        var chunkInfo = SharedFileUtility.LoadJsonFromFile<ChunkMeta>(metaFile);
                        if (chunkInfo == null) continue;

                        if (File.Exists(chunkInfo.chunkFilePath))
                        {
                            chunks.Add(chunkInfo);
                        }
                        else
                        {
                            SharedFileUtility.DeleteFileSafe(metaFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to load chunk meta from {metaFile}: {ex.Message}");
                    }
                }

                chunks.Sort((a, b) => a.sequence.CompareTo(b.sequence));
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load existing chunks: {ex.Message}");
            }

            return chunks;
        }
    }
}
