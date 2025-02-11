using UnityEngine;

namespace JahroConsole.View.InputHandling
{
    internal class LegacyInputProvider : IJahroInputProvider
    {
        public bool GetKeyDown(KeyCode keyCode)
        {
            return Input.GetKeyDown(keyCode);
        }

        public bool GetKey(KeyCode keyCode)
        {
            return Input.GetKey(keyCode);
        }

        public int TouchCount()
        {
            return Input.touches.Length;
        }

        public bool IsTouchEnded(int index)
        {
            return Input.GetTouch(index).phase == TouchPhase.Ended;
        }

        public Vector2 GetTouchPosition(int index)
        {
            return Input.GetTouch(index).position;
        }
    }
}