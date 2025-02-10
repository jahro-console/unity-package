using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
#endif

namespace JahroConsole.View.InputHandling
{
      public class JahroInput
      {
            public static bool GetKeyDown(KeyCode keyCode)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                  return Input.GetKeyDown(keyCode);
#endif

#if ENABLE_INPUT_SYSTEM
#pragma warning disable CS0162 // Unreachable code detected
                  return UnityEngine.InputSystem.Keyboard.current[keyCode.AsNewInputSystemKey()].wasPressedThisFrame;
#pragma warning restore CS0162 // Unreachable code detected
#endif
            }

            public static bool GetKey(KeyCode keyCode)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                  return Input.GetKey(keyCode);
#endif

#if ENABLE_INPUT_SYSTEM
#pragma warning disable CS0162 // Unreachable code detected
                  return UnityEngine.InputSystem.Keyboard.current[keyCode.AsNewInputSystemKey()].isPressed;
#pragma warning restore CS0162 // Unreachable code detected
#endif
            }

            public static int TouchCount()
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                  return Input.touches.Length;
#elif ENABLE_INPUT_SYSTEM
                  if (EnhancedTouchSupport.enabled)
                  {
                        return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;
                  }
#else
                  return 0;
#endif
            }

            public static bool IsTouchEnded(int index)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                  return Input.GetTouch(index).phase == TouchPhase.Ended;
#endif

#if ENABLE_INPUT_SYSTEM
#pragma warning disable CS0162 // Unreachable code detected
                  return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0].phase == UnityEngine.InputSystem.TouchPhase.Ended;
#pragma warning restore CS0162 // Unreachable code detected
#endif
            }

            public static Vector2 GetTouchPosition(int index)
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                  return Input.GetTouch(index).position;
#endif

#if ENABLE_INPUT_SYSTEM
#pragma warning disable CS0162 // Unreachable code detected
                  return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[index].screenPosition;
#pragma warning restore CS0162 // Unreachable code detected
#endif
            }
      }
}