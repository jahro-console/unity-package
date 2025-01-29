using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class CanvasScalingBehaviour : MonoBehaviour
    {
        internal enum ScaleMode
        {
            Small = 0,
            Default = 1,
            Large = 2
        }

        private ScaleMode _currentMode = ScaleMode.Default;

        internal const float SMALL_SCALE = 0f;

        internal const float DEFAULT_SCALE = 0.75f;

        internal const float LARGE_SCALE = 1.2f;

        private readonly Vector2 minResolution = new Vector2(678, 418);

        private float currentScale;

        private Vector2 initialReferenceResolution;

        public CanvasScaler CanvasScaler;

        public event Action<float> OnScaleChanged;

        private void Awake()
        {
            initialReferenceResolution = minResolution;

            currentScale = float.MinValue;
        }

        internal void SwitchToMode(ScaleMode mode)
        {
            _currentMode = mode;
            switch (_currentMode)
            {
                case ScaleMode.Small:
                    SetScale(SMALL_SCALE);
                    break;
                case ScaleMode.Default:
                    SetScale(DEFAULT_SCALE);
                    break;
                case ScaleMode.Large:
                    SetScale(LARGE_SCALE);
                    break;
            }
        }

        internal int GetCurrentMode()
        {
            return (int)_currentMode;
        }

        private void SetScale(float scaleValue)
        {
            if (currentScale == scaleValue)
            {
                return;
            }

            currentScale = scaleValue;
            CanvasScaler.referenceResolution = initialReferenceResolution * (2 - currentScale);
            StartCoroutine(EventFireScale());
        }

        private IEnumerator EventFireScale()
        {
            yield return new WaitForEndOfFrame();
            OnScaleChanged?.Invoke(currentScale);
        }
    }
}