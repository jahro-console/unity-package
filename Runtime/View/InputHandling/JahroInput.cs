using UnityEngine;

namespace JahroConsole.View.InputHandling
{
      public static class JahroInput
      {
            private static IJahroInputProvider _inputProvider;

            static JahroInput()
            {
#if ENABLE_LEGACY_INPUT_MANAGER
                  _inputProvider = new LegacyInputProvider();
#elif ENABLE_INPUT_SYSTEM
                  _inputProvider = new InputProvider();
#endif
            }

            public static bool GetKeyDown(KeyCode keyCode)
            {
                  return _inputProvider.GetKeyDown(keyCode);
            }

            public static bool GetKey(KeyCode keyCode)
            {
                  return _inputProvider.GetKey(keyCode);
            }

            public static int TouchCount()
            {
                  return _inputProvider.TouchCount();
            }

            public static bool IsTouchEnded(int index)
            {
                  return _inputProvider.IsTouchEnded(index);
            }

            public static Vector2 GetTouchPosition(int index)
            {
                  return _inputProvider.GetTouchPosition(index);
            }
      }
}