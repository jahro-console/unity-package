using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JahroConsole.Core.Logging;
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
                        JahroLogger.LogWarning($"[{JahroLogTag.Chunks}] Chunk meta load failed — path={metaFile}, error={ex.Message}. Chunk will be skipped.");
                    }
                }

                chunks.Sort((a, b) => a.sequence.CompareTo(b.sequence));
            }
            catch (Exception ex)
            {
                JahroLogger.LogError($"[{JahroLogTag.Chunks}] Failed to load existing chunks — error={ex.Message}");
            }

            return chunks;
        }
    }
}
