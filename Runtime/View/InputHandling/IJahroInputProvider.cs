using UnityEngine;

namespace JahroConsole.View.InputHandling
{
    internal interface IJahroInputProvider
    {
        bool GetKeyDown(KeyCode keyCode);
        bool GetKey(KeyCode keyCode);
        int TouchCount();
        bool IsTouchEnded(int index);
        Vector2 GetTouchPosition(int index);
    }
}