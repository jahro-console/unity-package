using System;
using System.Collections;
using Jahro.Core.Utilities;
using UnityEngine;

namespace Jahro.Core.Commands
{
    [Serializable]
    internal struct CommandEntryData
    {
        [SerializeField]
        internal bool Favorite;

        [SerializeField]
        internal string SimpleName;

        [SerializeField]
        internal string[] DefaultValues; 

        [SerializeField]
        internal string LastUsed;

        internal static CommandEntryData ExtractData(ConsoleCommandEntry entry)
        {
            var dataEntry = new CommandEntryData();
            dataEntry.SimpleName = entry.SimpleName;
            dataEntry.Favorite = entry.Favorite;
            dataEntry.LastUsed = entry.LastExecuted.ToString();

            var parameters = entry.LatestParams;
            if (parameters != null)
            {
                string[] convertedParams = new string[parameters.Length];
                for(int i=0; i<parameters.Length; i++)
                {
                    var param = parameters[i];
                    if (param == null)
                    {
                        continue;
                    }

                    if (param is Array)
                    {
                        string concatResult = "";
                        var cParam = param as IEnumerable;
                        foreach(var t in cParam)
                        {
                            concatResult += t.ToString() + "|";
                        }
                        convertedParams[i] = concatResult;
                    }
                    else
                    {
                        convertedParams[i] = param.ToString();
                    }
                }
                dataEntry.DefaultValues = convertedParams;
            }

            return dataEntry;
        }

        internal static void ApplyData(CommandEntryData entryData, ConsoleCommandEntry entry)
        {
            entry.SetFavorite(entryData.Favorite, true);

            string[] defaultValues = entryData.DefaultValues;
            var parameters = entry.MethodInfo.GetParameters();
            entry.LatestParams = null;

            if (string.IsNullOrEmpty(entryData.LastUsed) == false && DateTime.TryParse(entryData.LastUsed, out var parsedDateTime))
            {
                entry.LastExecuted = parsedDateTime;
            }

            if (defaultValues == null || defaultValues.Length != parameters.Length)
            {    
                return;
            }
            
            object[] resultArray = new object[parameters.Length];

            for(int i=0; i<parameters.Length; i++)
            {
                var param = parameters[i];
                var value = defaultValues[i];
                object result = null;

                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                if (param.ParameterType.IsPrimitive)
                {   
                    if(ConsoleCommandsParamsMapper.MapPrimitiveEntryParams(param.ParameterType, value, out object res))
                    {
                        result = res;
                    }
                    else
                    {
                        Debug.LogWarning("Couldn't map primitive type from string to object -> " + value);
                        return;
                    }
                }
                else if (param.ParameterType.Equals(typeof(string)))
                {
                    result = value;
                }
                else if (param.ParameterType.Equals(typeof(Vector3)))
                {
                    result = TypesConverter.CustomVector3StringToObject(value);
                }
                else if (param.ParameterType.Equals(typeof(Vector2)))
                {
                    result = TypesConverter.CustomVector2StringToObject(value);
                }
                else if (param.ParameterType.IsArray)
                {
                    if (ConsoleCommandsParamsMapper.MapCustomStringArrayToObject(param, value, out object res))
                    {
                        result = res;
                    }
                    else
                    {
                        Debug.LogWarning("Couldn't map array from string to object -> " + value);
                        return;
                    }
                }
                else if (param.ParameterType.IsEnum)
                {
                    result = Enum.Parse(param.ParameterType, value);
                }

                resultArray[i] = result;
            }
            
            entry.LatestParams = resultArray;
        }
    }
}