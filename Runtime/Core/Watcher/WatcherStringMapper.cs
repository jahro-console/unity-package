using System;
using System.Text;
using UnityEngine;

namespace JahroConsole.Core.Watcher
{
    internal static class WatcherStringMapper
    {
        internal static void MapValueToShortString(ConsoleWatcherEntry entry, object obj, StringBuilder stringBuilder, out int requiredSize)
        {
            int limit = 0;
            requiredSize = 1;
            stringBuilder.Clear();

            if (obj is null || obj.Equals(null))
            {
                stringBuilder.Append("<color=#FF5C01><b>null</b></color>");
                return;
            }

            var entryType = entry.GetMemberType();
            if (obj is Exception)
            {
                Exception e = (Exception)obj;
                stringBuilder.AppendFormat("<color=#FA002D><b>{0}</b></color>", e.GetType().Name);
                limit = 2;
            }
            else if (entryType.IsPrimitive)
            {
                stringBuilder.Append(obj.ToString());
                requiredSize = 1;
                limit = 1;
            }
            else if (entryType == typeof(string))
            {
                string objStr = obj.ToString();
                stringBuilder.Append(objStr.Substring(0, Math.Min(49, objStr.Length)));
            }
            else if (entryType.IsArray)
            {
                Array array = (Array)obj;
                string typeName = entryType.GetElementType().Name;
                stringBuilder.AppendFormat("{0}[{1}]", typeName, array.Length);
            }
            else if (entryType == typeof(Vector3))
            {
                Vector3 q = (Vector3)obj;
                stringBuilder.AppendFormat("x{0:F2} y{1:F2} z{2:F2}", q.x, q.y, q.z);
            }
            else if (entryType == typeof(Vector2))
            {
                Vector2 q = (Vector2)obj;
                stringBuilder.AppendFormat("x{0:F2} y{1:F2}", q.x, q.y);
            }
            else if (entryType == typeof(Quaternion))
            {
                Quaternion q = (Quaternion)obj;
                stringBuilder.AppendFormat("w{0:F2} x{1:F2} y{2:F2} z{3:F2}", q.w, q.x, q.y, q.z);
                limit = 2;
            }
            else if (entryType == typeof(Transform))
            {
                Transform transform = (Transform)obj;
                stringBuilder.AppendFormat("{0}/{1}", entryType.Name, transform.gameObject.name);
                limit = 2;
            }
            else if (entryType == typeof(RectTransform))
            {
                RectTransform transform = (RectTransform)obj;
                stringBuilder.AppendFormat("{0}/{1}", entryType.Name, transform.gameObject.name);
                limit = 2;
            }
            else if (entryType == typeof(GameObject))
            {
                GameObject gameObject = (GameObject)obj;
                stringBuilder.Append(gameObject.name);
                limit = 2;
            }
            else if (entryType == typeof(Rigidbody))
            {
                Rigidbody rigidbody = (Rigidbody)obj;
                stringBuilder.AppendFormat("{0}/{1}", entryType.Name, rigidbody.gameObject.name);
                limit = 2;
            }
            else if (entryType == typeof(Collider))
            {
                Collider collider = (Collider)obj;
                stringBuilder.AppendFormat("{0}/{1}", entryType.Name, collider.gameObject.name);
                limit = 2;
            }
            else if (entryType == typeof(AudioSource))
            {
                AudioSource audioSource = (AudioSource)obj;
                stringBuilder.AppendFormat("{0}/{1}", entryType.Name, audioSource.gameObject.name);
                limit = 2;
            }
            else if (entryType == typeof(Animator))
            {
                Animator go = (Animator)obj;
                stringBuilder.AppendFormat("{0}/{1}", entryType.Name, go.gameObject.name);
                limit = 2;
            }
            else if (entryType == typeof(Renderer))
            {
                Renderer go = (Renderer)obj;
                stringBuilder.AppendFormat("{0}/{1}", entryType.Name, go.gameObject.name);
                limit = 2;
            }
            else if (entryType == typeof(Camera))
            {
                Camera go = (Camera)obj;
                stringBuilder.AppendFormat("{0}/{1}", entryType.Name, go.gameObject.name);
                limit = 2;
            }
            else if (entryType == typeof(Light))
            {
                Light go = (Light)obj;
                stringBuilder.AppendFormat("{0}/{1}", entryType.Name, go.gameObject.name);
                limit = 2;
            }
            else if (entryType == typeof(Canvas))
            {
                Canvas go = (Canvas)obj;
                stringBuilder.AppendFormat("{0}/{1}", entryType.Name, go.gameObject.name);
                limit = 2;
            }
            else
            {
                string objStr = obj.ToString();
                stringBuilder.Append(objStr.Substring(0, Math.Min(49, objStr.Length)));
            }

            if (stringBuilder.Length > 25)
            {
                requiredSize = 3;
            }
            else if (stringBuilder.Length > 12)
            {
                requiredSize = 2;
            }
            else
            {
                requiredSize = 1;
            }

            if (limit != 0) requiredSize = Math.Min(requiredSize, limit);
        }

        internal static void MapValueToDetailsString(ConsoleWatcherEntry entry, object obj, StringBuilder stringBuilder)
        {
            stringBuilder.Clear();
            if (obj is null || obj.Equals(null))
            {
                stringBuilder.Append("null");
                return;
            }

            var entryType = obj.GetType();
            if (entryType.IsPrimitive)
            {
                stringBuilder.Append(obj.ToString());
            }
            else if (entryType.IsArray)
            {
                MapArray(obj, stringBuilder);
            }
            else if (entryType == typeof(Vector3))
            {
                MapVector3(obj, stringBuilder);
            }
            else if (entryType == typeof(Vector2))
            {
                MapVector2(obj, stringBuilder);
            }
            else if (entryType == typeof(Quaternion))
            {
                MapQuaternion(obj, stringBuilder);
            }
            else if (entryType == typeof(Transform))
            {
                MapTransform(obj, stringBuilder);
            }
            else if (entryType == typeof(RectTransform))
            {
                MapRectTransform(obj, stringBuilder);
            }
            else if (entryType == typeof(GameObject))
            {
                MapGameObject(obj, stringBuilder);
            }
            else if (entryType == typeof(Rigidbody2D))
            {
                MapRigidbody2D(obj, stringBuilder);
            }
            else if (entryType == typeof(Collider2D))
            {
                MapCollider2D(obj, stringBuilder);
            }
            else if (entryType == typeof(Rigidbody))
            {
                MapRigidbody(obj, stringBuilder);
            }
            else if (entryType == typeof(Collider))
            {
                MapCollider(obj, stringBuilder);
            }
            else if (entryType == typeof(AudioSource))
            {
                MapAudioSource(obj, stringBuilder);
            }
            else if (entryType == typeof(Animator))
            {
                MapAnimator(obj, stringBuilder);
            }
            else if (entryType == typeof(Renderer))
            {
                MapRenderer(obj, stringBuilder);
            }
            else if (entryType == typeof(Camera))
            {
                MapCamera(obj, stringBuilder);
            }
            else if (entryType == typeof(Light))
            {
                MapLight(obj, stringBuilder);
            }
            else if (entryType == typeof(Canvas))
            {
                MapCanvas(obj, stringBuilder);
            }
            else
            {
                stringBuilder.Append(obj.ToString());
            }
        }

        private static void MapArray(object obj, StringBuilder stringBuilder)
        {
            Array array = (Array)obj;
            stringBuilder.Append("Array Length[").Append(array.Length).Append("]").AppendLine();
            foreach (var i in array)
            {
                stringBuilder.Append(i.ToString()).Append("; ");
            }
        }

        private static void MapVector2(object obj, StringBuilder stringBuilder)
        {
            Vector2 vector = (Vector2)obj;
            stringBuilder.Append("X: ").Append(vector.x).AppendLine();
            stringBuilder.Append("Y: ").Append(vector.y).AppendLine();
            stringBuilder.Append("Magnitude: ").Append(vector.magnitude).AppendLine();
            stringBuilder.Append("Normilized: ").Append(vector.normalized.ToString());
        }

        private static void MapVector3(object obj, StringBuilder stringBuilder)
        {
            Vector3 vector = (Vector3)obj;
            stringBuilder.Append("X: ").Append(vector.x).AppendLine();
            stringBuilder.Append("Y: ").Append(vector.y).AppendLine();
            stringBuilder.Append("Z: ").Append(vector.z).AppendLine();
            stringBuilder.Append("Magnitude: ").Append(vector.magnitude).AppendLine();
            stringBuilder.Append("Normilized: ").Append(vector.normalized.ToString());
        }

        private static void MapQuaternion(object obj, StringBuilder stringBuilder)
        {
            Quaternion quaternion = (Quaternion)obj;
            stringBuilder.Append("W: ").Append(quaternion.w).AppendLine();
            stringBuilder.Append("X: ").Append(quaternion.x).AppendLine();
            stringBuilder.Append("Y: ").Append(quaternion.y).AppendLine();
            stringBuilder.Append("Z: ").Append(quaternion.z).AppendLine();
            stringBuilder.Append("Euler Angles: ").Append(quaternion.eulerAngles.ToString()).AppendLine();
            stringBuilder.Append("Normilized: ").Append(quaternion.normalized.ToString());
        }

        private static void MapTransform(object obj, StringBuilder stringBuilder)
        {
            Transform transform = (Transform)obj;
            stringBuilder.Append("Position: ").Append(transform.position.ToString()).AppendLine();
            stringBuilder.Append("Rotation: ").Append(transform.rotation.ToString()).AppendLine();
            stringBuilder.Append("Local Scale: ").Append(transform.localScale).AppendLine();
            stringBuilder.Append("Child Count: ").Append(transform.childCount).AppendLine();
        }

        private static void MapRectTransform(object obj, StringBuilder stringBuilder)
        {
            RectTransform transform = (RectTransform)obj;
            stringBuilder.Append("Anc.Position: ").Append(transform.anchoredPosition.ToString()).AppendLine();
            stringBuilder.Append("Pivot: ").Append(transform.pivot).AppendLine();
            stringBuilder.Append("Anc.Min: ").Append(transform.anchorMin).AppendLine();
            stringBuilder.Append("Anc.Max: ").Append(transform.anchorMax).AppendLine();
            stringBuilder.Append("Rotation: ").Append(transform.rotation).AppendLine();
            stringBuilder.Append("Local Scale: ").Append(transform.localScale).AppendLine();
            stringBuilder.Append("Child Count: ").Append(transform.childCount).AppendLine();
        }

        private static void MapGameObject(object obj, StringBuilder stringBuilder)
        {
            GameObject gameobject = (GameObject)obj;
            stringBuilder.Append("Name: ").Append(gameobject.name).AppendLine();
            stringBuilder.Append("Active Hier: ").Append(gameobject.activeInHierarchy).AppendLine();
            stringBuilder.Append("Active Self: ").Append(gameobject.activeSelf).AppendLine();
            stringBuilder.Append("Scene Name: ").Append(gameobject.scene.name).AppendLine();
            stringBuilder.Append("Tag: ").Append(gameobject.tag).AppendLine().AppendLine();

            Transform transform = gameobject.transform;
            stringBuilder.Append("Transform: ").AppendLine();
            MapTransform(transform, stringBuilder);
        }

        private static void MapRigidbody(object obj, StringBuilder stringBuilder)
        {
            Rigidbody rigidbody = (Rigidbody)obj;
            stringBuilder.Append("Mass: ").Append(rigidbody.mass).AppendLine();
            stringBuilder.Append("Is Kinematic: ").Append(rigidbody.isKinematic).AppendLine();
            stringBuilder.Append("Use Gravity: ").Append(rigidbody.useGravity).AppendLine();
            stringBuilder.Append("Angular Velocity: ").Append(rigidbody.angularVelocity).AppendLine();
        }

        private static void MapCollider(object obj, StringBuilder stringBuilder)
        {
            Collider collider = (Collider)obj;
            stringBuilder.Append("Is Trigger: ").Append(collider.isTrigger).AppendLine();
            stringBuilder.Append("Material: ").Append(collider.material.name).AppendLine();
            stringBuilder.Append("Center: ").Append(collider.bounds.center.ToString()).AppendLine();
            stringBuilder.Append("Size: ").Append(collider.bounds.size.ToString()).AppendLine();
        }

        private static void MapRigidbody2D(object obj, StringBuilder stringBuilder)
        {
            Rigidbody2D rigidbody = (Rigidbody2D)obj;
            stringBuilder.Append("Mass: ").Append(rigidbody.mass).AppendLine();
            stringBuilder.Append("Is Kinematic: ").Append(rigidbody.bodyType == RigidbodyType2D.Kinematic).AppendLine();
            stringBuilder.Append("Gravity Scale: ").Append(rigidbody.gravityScale).AppendLine();
            stringBuilder.Append("Angular Velocity: ").Append(rigidbody.angularVelocity).AppendLine();
        }

        private static void MapCollider2D(object obj, StringBuilder stringBuilder)
        {
            Collider2D collider = (Collider2D)obj;
            stringBuilder.Append("Is Trigger: ").Append(collider.isTrigger).AppendLine();
            stringBuilder.Append("Bounciness: ").Append(collider.bounciness).AppendLine();
            stringBuilder.Append("Friction: ").Append(collider.friction).AppendLine();
            stringBuilder.Append("Center: ").Append(collider.bounds.center.ToString()).AppendLine();
            stringBuilder.Append("Size: ").Append(collider.bounds.size.ToString()).AppendLine();
        }

        private static void MapAudioSource(object obj, StringBuilder stringBuilder)
        {
            AudioSource audioSource = (AudioSource)obj;
            stringBuilder.Append("Clip: ").Append(audioSource.clip.name).AppendLine();
            stringBuilder.Append("Volume: ").Append(audioSource.volume).AppendLine();
            stringBuilder.Append("Loop: ").Append(audioSource.loop).AppendLine();
            stringBuilder.Append("Pitch: ").Append(audioSource.pitch).AppendLine();
            stringBuilder.Append("Mute: ").Append(audioSource.mute).AppendLine();
        }

        private static void MapAnimator(object obj, StringBuilder stringBuilder)
        {
            Animator animator = (Animator)obj;

            stringBuilder.Append("Current State: ").Append(animator.GetCurrentAnimatorStateInfo(0).shortNameHash).AppendLine();
            stringBuilder.Append("Speed: ").Append(animator.speed).AppendLine();
            stringBuilder.Append("Layer Count: ").Append(animator.layerCount).AppendLine();
        }

        private static void MapRenderer(object obj, StringBuilder stringBuilder)
        {
            MeshRenderer renderer = (MeshRenderer)obj;
            stringBuilder.Append("Material: ").Append(renderer.material).AppendLine();
            stringBuilder.Append("Cast Shadows: ").Append(renderer.shadowCastingMode).AppendLine();
            stringBuilder.Append("Receive Shadows: ").Append(renderer.receiveShadows).AppendLine();
        }

        private static void MapCamera(object obj, StringBuilder stringBuilder)
        {
            Camera camera = (Camera)obj;
            stringBuilder.Append("Field of View: ").Append(camera.fieldOfView).AppendLine();
            stringBuilder.Append("Near Clip Plane: ").Append(camera.nearClipPlane).AppendLine();
            stringBuilder.Append("Far Clip Plane: ").Append(camera.farClipPlane).AppendLine();
            stringBuilder.Append("Aspect : ").Append(camera.aspect).AppendLine();
        }

        private static void MapLight(object obj, StringBuilder stringBuilder)
        {
            Light light = (Light)obj;
            stringBuilder.Append("Type: ").Append(light.type).AppendLine();
            stringBuilder.Append("Intensity: ").Append(light.intensity).AppendLine();
            stringBuilder.Append("Shadows: ").Append(light.shadows).AppendLine();
        }

        private static void MapCanvas(object obj, StringBuilder stringBuilder)
        {
            Canvas canvas = (Canvas)obj;
            stringBuilder.Append("Render Mode: ").Append(canvas.renderMode).AppendLine();
            stringBuilder.Append("Pixel Perfect: ").Append(canvas.pixelPerfect).AppendLine();
            stringBuilder.Append("Scale Factor: ").Append(canvas.scaleFactor).AppendLine();
        }

        //todo
        //texture2d
        //color
        //mesh
    }
}