using System;
using JahroPackage.Runtime.View.InputHandling;
using UnityEngine;

namespace Jahro.View
{
    internal class GestureTrackerTap : MonoBehaviour
    {
        public const int TAP_COUNT_LAUNCH = 4;

        public const float TAPS_DELTA = 0.5f;

        private int _tapCount;

        private float _prevTapTimestemp;

        public Action OnTapsTracked = delegate{}; 

        private void Update()
        {
            if (JahroInput.TouchCount() <= 0)
                return;

            if (JahroInput.IsTouchEnded(0) && JahroInput.GetTouchPosition(0).y > Screen.height * 0.75f)
            {
                float timeSincePreviousClick = Time.realtimeSinceStartup - _prevTapTimestemp;        
                if (timeSincePreviousClick < TAPS_DELTA)
                {
                    _tapCount++;
                    if (_tapCount == TAP_COUNT_LAUNCH)
                    {
                        OnTapsTracked();
                    }
                }
                else
                {
                    _tapCount = 1;
                }
                _prevTapTimestemp = Time.realtimeSinceStartup;
            }
        }
    }
}