using System;
using System.Collections;
using System.IO;
using JahroConsole.Core.Data;
using JahroConsole.Core.Notifications;
using JahroConsole.Core.Snapshots;
using UnityEngine;

namespace JahroConsole.Core.Snapshots
{
    internal class ScreenShooter
    {
        internal static IEnumerator Capture(SnapshotSession session, ScreenshotMeta screenshotMeta)
        {
            yield return new WaitForEndOfFrame();

            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();

            var byteArray = texture.EncodeToJPG(75);
            DateTime now = DateTime.UtcNow;
            string date = now.ToString("yyyy-MM-dd");
            string time = now.ToString("HH-mm-ss-fff");
            string filename = $"jahro-screenshot-{date}-at-{time}.jpg";

            string filePath = Path.Combine(session.screenshotsFolderPath, filename);

            File.WriteAllBytes(filePath, byteArray);
            screenshotMeta.id = Guid.NewGuid().ToString();
            screenshotMeta.filepath = filePath;
            screenshotMeta.metaFilePath = filePath + ".meta";
            screenshotMeta.createdAt = now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            screenshotMeta.status = ScreenshotMeta.ScreenshotState.Saved;
            screenshotMeta.sizeBytes = new FileInfo(filePath).Length;
            screenshotMeta.lastError = null;
            screenshotMeta.lastUploadAt = null;
            ScreenshotFileUtility.SaveScreenshotMeta(screenshotMeta);
        }
    }
}
