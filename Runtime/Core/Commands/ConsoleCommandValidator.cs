using System.Collections.Generic;
using Jahro.Core.Registry;

namespace Jahro.Core.Commands
{
    internal static class ConsoleCommandValidator
    {
        internal static string SplitInput(string input, out string[] parameters)
        {

            var names = ConsoleCommandsRegistry.Holder.GetCommandsNames();
            var trimmedInput = input.Trim();

            foreach (var name in names)
            {
                if (input.StartsWith(name) && (trimmedInput.Length == name.Length || trimmedInput.Substring(name.Length, 1) == " "))
                {
                    int lastPos = trimmedInput.IndexOf(name) + name.Length;
                    var tmpParams = trimmedInput.Substring(lastPos).Trim().Split(' ');
                    var resultParams = new List<string>();

                    foreach (var para in tmpParams)
                    {
                        if (!string.IsNullOrEmpty(para))
                        {
                            resultParams.Add(para);
                        }
                    }
                    parameters = resultParams.ToArray();
                    resultParams.Clear();
                    return trimmedInput.Substring(0, lastPos);
                }
            }

            var commandComponents = trimmedInput.Split(' ');
            var command = commandComponents[0];

            parameters = new string[commandComponents.Length - 1];
            for (int i = 1; i <= parameters.Length; i++)
            {
                parameters[i - 1] = commandComponents[i];
            }
            return command;
        }
    }
}