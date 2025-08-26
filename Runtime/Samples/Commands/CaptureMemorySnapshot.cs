using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Profiling;

namespace JahroConsole.Samples
{
    public static class CaptureMemorySnapshot
    {
        [JahroCommand(name: "capture-memory-snapshot", group: "Samples", description: "Captures current memory usage snapshot with reliable Unity API data")]
        public static string CaptureMemorySnapshotCommand()
        {
            try
            {
                return CaptureSnapshot();
            }
            catch (Exception ex)
            {
                return $"Memory snapshot failed: {ex.Message}";
            }
        }

        private static string CaptureSnapshot()
        {
            var stringBuilder = new StringBuilder(1024);

            long managedMemory = GC.GetTotalMemory(false);
            long nativeMemory = Profiler.GetTotalAllocatedMemoryLong();
            long totalMemory = managedMemory + nativeMemory;

            stringBuilder.AppendLine("Memory Snapshot");
            stringBuilder.AppendLine("--------------------------------");
            stringBuilder.AppendLine($"Managed: {FormatBytes(managedMemory)}");
            stringBuilder.AppendLine($"Native: {FormatBytes(nativeMemory)}");
            stringBuilder.AppendLine($"Total: {FormatBytes(totalMemory)}");
            stringBuilder.AppendLine();

            AnalyzeUnityObjects(stringBuilder);
            return stringBuilder.ToString();
        }

        private static void AnalyzeUnityObjects(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine("Unity Object Counts:");

            var textures = Resources.FindObjectsOfTypeAll<Texture2D>();
            stringBuilder.AppendLine($" • Textures: {textures.Length} instances");

            var meshes = Resources.FindObjectsOfTypeAll<Mesh>();
            stringBuilder.AppendLine($" • Meshes: {meshes.Length} instances");

            var audioClips = Resources.FindObjectsOfTypeAll<AudioClip>();
            stringBuilder.AppendLine($" • Audio Clips: {audioClips.Length} instances");

            var materials = Resources.FindObjectsOfTypeAll<Material>();
            stringBuilder.AppendLine($" • Materials: {materials.Length} instances");

            var shaders = Resources.FindObjectsOfTypeAll<Shader>();
            stringBuilder.AppendLine($" • Shaders: {shaders.Length} instances");

            var animations = Resources.FindObjectsOfTypeAll<AnimationClip>();
            stringBuilder.AppendLine($" • Animation Clips: {animations.Length} instances");

            var gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            stringBuilder.AppendLine($" • GameObjects: {gameObjects.Length} instances");

            var particleSystems = Resources.FindObjectsOfTypeAll<ParticleSystem>();
            stringBuilder.AppendLine($" • Particle Systems: {particleSystems.Length} instances");

            var fonts = Resources.FindObjectsOfTypeAll<Font>();
            stringBuilder.AppendLine($" • Fonts: {fonts.Length} instances");

            var sprites = Resources.FindObjectsOfTypeAll<Sprite>();
            stringBuilder.AppendLine($" • Sprites: {sprites.Length} instances");

            var renderers = Resources.FindObjectsOfTypeAll<Renderer>();
            stringBuilder.AppendLine($" • Renderers: {renderers.Length} instances");

            var colliders = Resources.FindObjectsOfTypeAll<Collider>();
            stringBuilder.AppendLine($" • Colliders: {colliders.Length} instances");

            var lights = Resources.FindObjectsOfTypeAll<Light>();
            stringBuilder.AppendLine($" • Lights: {lights.Length} instances");

            var cameras = Resources.FindObjectsOfTypeAll<Camera>();
            stringBuilder.AppendLine($" • Cameras: {cameras.Length} instances");

            var rigidbodies = Resources.FindObjectsOfTypeAll<Rigidbody>();
            stringBuilder.AppendLine($" • Rigidbodies: {rigidbodies.Length} instances");

            var audioSources = Resources.FindObjectsOfTypeAll<AudioSource>();
            stringBuilder.AppendLine($" • Audio Sources: {audioSources.Length} instances");

            var canvas = Resources.FindObjectsOfTypeAll<Canvas>();
            stringBuilder.AppendLine($" • Canvas: {canvas.Length} instances");

            var uiElements = Resources.FindObjectsOfTypeAll<UnityEngine.UI.Graphic>();
            stringBuilder.AppendLine($" • UI Elements: {uiElements.Length} instances");

            stringBuilder.AppendLine();
        }

        private static string FormatBytes(long bytes)
        {
            const double mb = 1024.0 * 1024.0;
            return $"{bytes / mb:F1} MB";
        }
    }
}
