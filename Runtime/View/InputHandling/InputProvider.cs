
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

namespace JahroConsole.View.InputHandling
{
    internal class InputProvider : IJahroInputProvider
    {
        public bool GetKeyDown(KeyCode keyCode)
        {
            return Keyboard.current[keyCode.AsNewInputSystemKey()].wasPressedThisFrame;
        }

        public bool GetKey(KeyCode keyCode)
        {
            return Keyboard.current[keyCode.AsNewInputSystemKey()].isPressed;
        }

        public int TouchCount()
        {
            if (EnhancedTouchSupport.enabled)
            {
                return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;
            }
            return 0;
        }

        public bool IsTouchEnded(int index)
        {
            return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Ended;
        }

        public Vector2 GetTouchPosition(int index)
        {
            return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[index].screenPosition;
        }
    }
}
#endif