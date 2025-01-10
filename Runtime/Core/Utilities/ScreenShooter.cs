using System.Collections;
using System.IO;
using Jahro.Core.Data;
using Jahro.Core.Notifications;
using Jahro.Core.Snapshots;
using UnityEngine;

namespace Jahro.Core.Utilities
{
    internal class ScreenShooter
    {
        internal static IEnumerator Capture(SnapshotSession session)
        {
            yield return new WaitForEndOfFrame();

            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();

            var byteArray = texture.EncodeToJPG(75);
            string filename = FileManager.GetPathToScreenShot(session);

            File.WriteAllBytes(filename, byteArray);
            NotificationService.Instance.SendNotification(new Notification("Screenshot capruted"));
        }
    }
}