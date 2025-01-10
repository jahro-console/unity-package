using System;
using Jahro.Core.Logging;

namespace Jahro.Core.Commands
{
    internal static class ConsoleCommandsInvoker
    {

        internal class StaticInvoker : ICommandInvoker
        {
            void ICommandInvoker.InvokeCommand(ConsoleCommandEntry targetEntry, object[] entryParams)
            {
                object result = null;
                JahroLogger.LogCommand(targetEntry.Name, entryParams);

                try
                {
                    result = targetEntry.MethodInfo.Invoke(null, entryParams);
                }
                catch (Exception e)
                {
                    JahroLogger.LogException(e.Message, e);
                }
                finally
                {
                    if (result != null)
                        JahroLogger.LogCommandResult(targetEntry.Name, entryParams, result.ToString());
                    targetEntry.Executed(entryParams, result);
                }
            }
        }
        //????
        internal class GameObjectInvoker : ICommandInvoker
        {
            void ICommandInvoker.InvokeCommand(ConsoleCommandEntry targetEntry, object[] entryParams)
            {
                // List<object> results = new List<object>();
                // var entryObjects = UnityEngine.GameObject.FindObjectsOfType(targetEntry.MethodInfo.DeclaringType);

                // if (entryObjects.Length == 0)
                // {
                //     Jahro.LogError(string.Format(MessagesResource.LogCommandMonoObjectsNotFound, targetEntry.Name));
                //     return;
                // }

                // try
                // {
                //     if (entryObjects != null)
                //     {       
                //         foreach (var entryObject in entryObjects)
                //         {
                //             var result = targetEntry.MethodInfo.Invoke(entryObject, entryParams);
                //             results.Add(result);
                //         }
                //     }
                // }
                // catch (Exception e)
                // {
                //     Jahro.LogException(e.Message, e);
                // }
                // finally
                // {
                //     JahroLogger.LogCommandResult(targetEntry.Name, entryParams, results);
                //     targetEntry.Executed(entryParams);
                // }
            }
        }

        internal class ObjectInvoker : ICommandInvoker
        {
            void ICommandInvoker.InvokeCommand(ConsoleCommandEntry targetEntry, object[] entryParams)
            {
                object result = null;

                if (targetEntry.ReferenceObject == null)
                {
                    JahroLogger.LogError(string.Format(MessagesResource.LogCommandObjectNullReference, targetEntry.Name));
                    return;
                }

                JahroLogger.LogCommand(targetEntry.Name, entryParams);

                try
                {
                    result = targetEntry.MethodInfo.Invoke(targetEntry.ReferenceObject, entryParams);
                }
                catch (Exception e)
                {
                    JahroLogger.LogException(e.Message, e);
                }
                finally
                {
                    if (result != null)
                        JahroLogger.LogCommandResult(targetEntry.Name, entryParams, result.ToString());
                    targetEntry.Executed(entryParams, result);
                }
            }
        }

        internal class CallbackInvoker : ICommandInvoker
        {
            void ICommandInvoker.InvokeCommand(ConsoleCommandEntry targetEntry, object[] entryParams)
            {
                JahroLogger.LogCommand(targetEntry.Name, entryParams);

                try
                {
                    targetEntry.DelegateInfo.DynamicInvoke(entryParams);
                }
                catch (Exception e)
                {
                    JahroLogger.LogException(e.Message, e);
                }
                finally
                {
                    targetEntry.Executed(entryParams, null);
                }
            }
        }

        internal interface ICommandInvoker
        {
            internal void InvokeCommand(ConsoleCommandEntry targetEntry, object[] entryParams);
        }
    }
}
