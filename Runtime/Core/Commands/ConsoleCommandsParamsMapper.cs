using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace JahroConsole.Core.Commands
{
    internal static class ConsoleCommandsParamsMapper
    {


        /*
        Possible combination
        Method Params   |   Args
        Non param       |   Null
        String          |   args[] > string
        Bool            |   args[] > 0 | 1 > TRUE | FALSE
        String[]        |   args[] > string[]
        Int             |   args[] > int
        Float           |   args[] > float
        Int, Int        |   args[] > int, int
        Int[]           |   args[] > int[]
        Float[]         |   args[] > float[]
        Float, Float    |   args[] > float, float
        Int, Float <>   |   args[] > int, float <>
        Vector3         |   args[] > float, float, float
        ENUM            |   args[] > enum parse

        ""
        =====>
        string  0.1
        int     0.6
        float   0.4
        bool    0.9
        
        */

        internal static object[] MapParams(List<ConsoleCommandEntry> entries, string[] args, out ConsoleCommandEntry targetEntry)
        {
            targetEntry = null;
            object[] resultedParams = null;
            float maxScore = int.MinValue;
            foreach (var entry in entries)
            {
                var parms = PickEntry(entry, args, out float score);
                if (score > maxScore && score >= 0)
                {
                    maxScore = score;
                    targetEntry = entry;
                    resultedParams = parms;
                }
            }
            return resultedParams;
        }

        private static object[] PickEntry(ConsoleCommandEntry entry, string[] args, out float score)
        {
            object[] castedParams = null;

            var parametersInfos = entry.MethodInfo.GetParameters();

            if (parametersInfos.Length == 0)
            {
                if (args.Length != 0)
                {
                    score = -1f;
                    return null;
                }
                else
                {
                    score = 0.1f;
                    return null;
                }
            }
            else if (parametersInfos.Length == 1 && parametersInfos[0].ParameterType.IsPrimitive == false)
            {
                if (parametersInfos[0].ParameterType.IsArray)
                {
                    castedParams = PickArrayEntry(parametersInfos[0], args, out score);
                    return castedParams;
                }
                else if (parametersInfos[0].ParameterType == typeof(Vector3) && args.Length == 3)
                {
                    if (float.TryParse(args[0], out float x) && float.TryParse(args[1], out float y) && float.TryParse(args[2], out float z))
                    {
                        castedParams = new object[] { new Vector3(x, y, z) };
                        score = 0.9f;
                        return castedParams;
                    }
                    else
                    {
                        score = -1f;
                        return null;
                    }
                }
                else if (parametersInfos[0].ParameterType == typeof(Vector2) && args.Length == 2)
                {
                    if (float.TryParse(args[0], out float x) && float.TryParse(args[1], out float y))
                    {
                        castedParams = new object[] { new Vector2(x, y) };
                        score = 0.8f;
                        return castedParams;
                    }
                    else
                    {
                        score = -1f;
                        return null;
                    }
                }
                else if (parametersInfos[0].ParameterType == typeof(string) && args.Length >= 1)
                {

                    castedParams = new object[] { string.Concat(args) };
                    score = 0.3f;
                    return castedParams;
                }
                else if (parametersInfos[0].ParameterType.IsEnum && args.Length == 1)
                {
                    try
                    {
                        var result = Enum.Parse(parametersInfos[0].ParameterType, args[0], true);
                        castedParams = new object[] { result };
                    }
                    catch
                    {
                        score = -1f;
                        return null;
                    }

                    score = 0.8f;
                    return castedParams;
                }
            }
            else if (parametersInfos.Length == args.Length)
            {
                castedParams = new object[parametersInfos.Length];
                int index = 0;
                foreach (var parameterInfo in parametersInfos)
                {
                    if (parameterInfo.ParameterType.IsPrimitive)
                    {
                        bool casted = MapPrimitiveEntryParams(parameterInfo.ParameterType, args[index], out object result);
                        if (casted)
                        {
                            castedParams[index] = result;
                        }
                        else
                        {
                            score = -1f;
                            return null;
                        }
                    }
                    else
                    {
                        //Can't support (Primitive, NonPrimitive)
                        score = -1f;
                        return null;
                    }
                    index++;
                }
                score = 0.9f;
                return castedParams;
            }
            //Can't parse anything
            score = -1f;
            return null;
        }

        internal static object[] PickArrayEntry(ParameterInfo parameterInfo, string[] args, out float score)
        {
            if (args.Length == 0)
            {
                score = 0f;
                return null;
            }

            object[] castedParams = null;

            if (parameterInfo.ParameterType == typeof(string[]))
            {
                castedParams = new[] { args };
                score = 0.2f;
                return castedParams;
            }
            else if (parameterInfo.ParameterType == typeof(int[]))
            {
                int[] arr = new int[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    if (int.TryParse(args[i], out int t))
                    {
                        arr[i] = t;
                    }
                    else
                    {
                        score = -1f;
                        return null;
                    }
                }
                score = 1f;
                castedParams = new[] { arr };
                return castedParams;
            }
            else if (parameterInfo.ParameterType == typeof(float[]))
            {
                float[] arr = new float[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    if (float.TryParse(args[i], out float t))
                    {
                        arr[i] = t;
                    }
                    else
                    {
                        score = -1f;
                        return null;
                    }
                }
                score = 1f;
                castedParams = new[] { arr };
                return castedParams;
            }
            else if (parameterInfo.ParameterType == typeof(bool[]))
            {
                bool[] arr = new bool[args.Length];
                for (int i = 0; i < args.Length; i++)
                {
                    if (bool.TryParse(args[i], out bool t))
                    {
                        arr[i] = t;
                    }
                    else
                    {
                        score = -1f;
                        return null;
                    }
                }
                score = 1f;
                castedParams = new[] { arr };
                return castedParams;
            }

            score = -1f;
            return castedParams;
        }

        internal static bool MapPrimitiveEntryParams(Type type, string arg, out object result)
        {
            if (type.Equals(typeof(int)) && int.TryParse(arg, out int intResult))
            {
                result = intResult;
                return true;
            }
            else if (type.Equals(typeof(float)) && float.TryParse(arg, out float floatResult))
            {
                result = floatResult;
                return true;
            }
            else if (type.Equals(typeof(double)) && double.TryParse(arg, out double doubleResult))
            {
                result = doubleResult;
                return true;
            }
            else if (type.Equals(typeof(bool)) && bool.TryParse(arg, out bool boolResult))
            {
                result = boolResult;
                return true;
            }

            result = null;
            return false;
        }

        internal static bool MapCustomStringArrayToObject(ParameterInfo parameterInfo, string value, out object result)
        {
            string[] arrayValues = value.Split('|');
            var arrayType = parameterInfo.ParameterType;
            object[] resultArray = new object[arrayValues.Length - 1];

            Type itemType = null;
            if (arrayType.Equals(typeof(int[])))
            {
                itemType = typeof(int);
            }
            else if (arrayType.Equals(typeof(float[])))
            {
                itemType = typeof(float);
            }
            else if (arrayType.Equals(typeof(double[])))
            {
                itemType = typeof(double);
            }
            else if (arrayType.Equals(typeof(bool[])))
            {
                itemType = typeof(bool);
            }
            else if (arrayType.Equals(typeof(string[])))
            {
                result = arrayValues;
                return true;
            }
            else
            {
                result = null;
                return false;
            }

            for (int i = 0; i < resultArray.Length; i++)
            {
                string strValue = arrayValues[i];
                if (MapPrimitiveEntryParams(itemType, strValue, out object itemResult))
                {
                    resultArray[i] = itemResult;
                }
                else
                {
                    result = null;
                    return false;
                }
            }
            result = resultArray;
            return true;
        }

        private static bool IsOverriden()
        {
            return false;
        }

        internal static bool HasParams(MethodInfo methodInfo)
        {
            return methodInfo.GetParameters().Length != 0;
        }
    }
}