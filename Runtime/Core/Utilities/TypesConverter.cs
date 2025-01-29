using UnityEngine;

namespace JahroConsole.Core.Utilities
{
    internal static class TypesConverter
    {
        internal static bool ToBool(object obj)
        {
            return (bool)obj;
        }

        internal static bool ToBool(string obj)
        {
            return bool.Parse(obj);
        }

        internal static object ToObject(int value)
        {
            return value;
        }

        internal static object CustomVector3StringToObject(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Vector3.zero;
            }
            string[] values = value.Substring(1, value.Length - 2).Split(',');
            if (values == null || values.Length != 3)
            {
                return null;
            }
            if (float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y) && float.TryParse(values[2], out float z))
            {
                return new Vector3(x, y, z);
            }
            else
            {
                return null;
            }
        }

        internal static object CustomVector2StringToObject(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return Vector2.zero;
            }
            string[] values = value.Substring(1, value.Length - 2).Split(',');
            if (values == null || values.Length != 2)
            {
                return null;
            }

            if (float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y))
            {
                return new Vector2(x, y);
            }
            else
            {
                return null;
            }
        }
    }
}