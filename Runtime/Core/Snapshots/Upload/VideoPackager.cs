using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace JahroConsole.Core.Snapshots
{
    internal static class VideoPackager
    {
        internal static async Task<string> CreateFramePackageAsync(
            List<string> framePaths, CancellationToken cancellationToken)
        {
            var tempZipPath = Path.Combine(Path.GetTempPath(),
                $"frames-{Guid.NewGuid():N}.zip");

            await Task.Run(() =>
            {
                using var zipStream = new FileStream(tempZipPath, FileMode.Create,
                    FileAccess.Write, FileShare.None, 16 * 1024);
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

                byte[] buffer = new byte[16 * 1024];

                foreach (var framePath in framePaths)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string entryName = Path.GetFileName(framePath);
                    var entry = archive.CreateEntry(entryName, CompressionLevel.NoCompression);

                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(framePath, FileMode.Open,
                        FileAccess.Read, FileShare.Read, buffer.Length,
                        FileOptions.SequentialScan);

                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        entryStream.Write(buffer, 0, bytesRead);
                    }
                }
            }, cancellationToken);

            return tempZipPath;
        }

        /// <summary>
        /// Counter-based frame collection: avoids full directory scan by using
        /// the known frame number range from FrameWriterService counters.
        /// Glob pattern includes videoId to match the new filename format.
        /// </summary>
        internal static List<string> GetPendingFramesByCounter(
            string framesDirectory, int videoId, int lastUploadedFrame, int totalFramesCaptured)
        {
            var pending = new List<string>();
            for (int n = lastUploadedFrame + 1; n <= totalFramesCaptured; n++)
            {
                var files = Directory.GetFiles(framesDirectory, $"frame_{videoId:D3}_{n:D6}_*{FrameWriterService.FrameFileExtension}");
                if (files.Length == 0)
                    break;
                pending.Add(files[0]);
            }
            return pending;
        }

        internal static List<string> GetPendingFrames(
            string framesDirectory, int lastUploadedFrame)
        {
            var allFrames = Directory.GetFiles(framesDirectory, $"frame_*{FrameWriterService.FrameFileExtension}");
            Array.Sort(allFrames, StringComparer.Ordinal);

            var pending = new List<string>();
            foreach (var framePath in allFrames)
            {
                int frameNumber = ExtractFrameNumber(framePath);
                if (frameNumber > lastUploadedFrame)
                    pending.Add(framePath);
            }
            return pending;
        }

        internal static int ExtractVideoId(string framePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(framePath);
            string[] parts = fileName.Split('_');
            // New format: frame_{videoId}_{frameNumber}_{timestamp}
            if (parts.Length >= 4 && int.TryParse(parts[1], out int videoId))
                return videoId;
            return 0;
        }

        internal static int ExtractFrameNumber(string framePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(framePath);
            string[] parts = fileName.Split('_');

            // New format: frame_{videoId}_{frameNumber}_{timestamp}
            if (parts.Length >= 4 && int.TryParse(parts[2], out int newFormatNumber))
                return newFormatNumber;

            // Legacy format: frame_{frameNumber}_{timestamp}
            if (parts.Length >= 2 && int.TryParse(parts[1], out int legacyNumber))
                return legacyNumber;

            return 0;
        }
    }
}
