using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JahroConsole.Core.Commands;
using JahroConsole.Core.Context;
using JahroConsole.Core.Data;
using JahroConsole.Core.Logging;
using JahroConsole.Core.Watcher;
using UnityEngine;

namespace JahroConsole.Core.Registry
{
    internal static class ConsoleCommandsRegistry
    {
        internal static ConsoleCommandHolder Holder { get { return _holder; } }

        internal static ConsoleWatcher Watcher { get { return _watcher; } }

        public static bool Initialized { get; private set; }

        private static ConsoleCommandHolder _holder = new ConsoleCommandHolder();

        private static ConsoleWatcher _watcher = new ConsoleWatcher();

        internal static void Initialize(IProjectSettings projectSettings, JahroContext context)
        {
            var methods = RetrieveMethods(projectSettings);
            InitializeStaticConsoleCommands(methods, context);
        }

        private static List<MemberInfo> RetrieveMethods(IProjectSettings projectSettings)
        {
            List<MemberInfo> members = new List<MemberInfo>();
            var allAssmblies = System.AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly =>
                    !assembly.GetName().Name.StartsWith("Unity") &&
                    !assembly.GetName().Name.StartsWith("System") &&
                    !assembly.GetName().Name.StartsWith("mscorlib") &&
                    projectSettings.ActiveAssemblies.Contains(assembly.GetName().Name));

            foreach (var assembly in allAssmblies)
            {
                var methodsArray = assembly.GetTypes()
                    .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    .Where(m => m.GetCustomAttributes(typeof(JahroCommandAttribute), false).Length > 0);
                members.AddRange(methodsArray);

                var fieldsArray = assembly.GetTypes()
                    .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    .Where(m => m.GetCustomAttributes(typeof(JahroWatchAttribute), false).Length > 0);
                members.AddRange(fieldsArray);

                var propsArray = assembly.GetTypes()
                    .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                    .Where(m => m.GetCustomAttributes(typeof(JahroWatchAttribute), false).Length > 0);
                members.AddRange(propsArray);
            }
            return members;
        }

        private static void InitializeStaticConsoleCommands(List<MemberInfo> members, JahroContext context)
        {
            foreach (var memberInfo in members)
            {
                if (memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property)
                {
                    JahroWatchAttribute attribute = memberInfo.GetCustomAttributes(typeof(JahroWatchAttribute), false)[0] as JahroWatchAttribute;
                    if (string.IsNullOrWhiteSpace(attribute.Name))
                    {
                        attribute.Name = memberInfo.Name.TrimStart('_');
                    }
                    var entry = new ConsoleWatcherEntry(attribute.Name.Trim(), attribute.Description.Trim());
                    entry.SetMemberInfo(memberInfo);
                    Watcher.InitEntryToWatch(entry, attribute.GroupName.Trim());
                }
                else if (memberInfo.MemberType == MemberTypes.Method)
                {
                    JahroCommandAttribute attribute = memberInfo.GetCustomAttributes(typeof(JahroCommandAttribute), false)[0] as JahroCommandAttribute;
                    if (string.IsNullOrWhiteSpace(attribute.MethodName))
                    {
                        attribute.MethodName = memberInfo.Name;
                    }
                    attribute.MethodName = attribute.MethodName.Replace(" ", "");
                    var entry = new ConsoleCommandEntry(attribute.MethodName.Trim(), attribute.MethodDescription.Trim());
                    entry.SetMethodInfo(memberInfo as MethodInfo);
                    entry.SetInvoker(new ConsoleCommandsInvoker.StaticInvoker());
                    entry.SetRuntime(false);
                    Holder.InitCommandMethod(entry, attribute.GroupName.Trim());
                }
            }

            Holder.Initialize(ConsoleStorageController.Instance.ConsoleStorage, context);
            Watcher.Initialize(ConsoleStorageController.Instance.ConsoleStorage, context);
            Initialized = true;
        }

        internal static void InvokeCommand(ConsoleCommandEntry targetEntry, object[] entryParams)
        {
            targetEntry.Invoker.InvokeCommand(targetEntry, entryParams);
        }

        internal static void InvokeCommand(string name, string[] args)
        {
            var entries = Holder.GetCommandEntries(name, args);
            if (entries.Count > 0)
            {
                ConsoleCommandEntry targetEntry = null;
                var entryParams = ConsoleCommandsParamsMapper.MapParams(entries, args, out targetEntry);
                if (targetEntry == null)
                {
                    string entriesParams = "";
                    foreach (var entriesNames in entries)
                    {
                        entriesParams += entriesNames.GetReadableParameters() + "; ";
                    }
                    JahroLogger.LogError(string.Format(MessagesResource.LogCommandCastError, name, entriesParams));
                }
                else
                {
                    targetEntry.Invoker.InvokeCommand(targetEntry, entryParams);
                }
            }
            else
            {
                JahroLogger.LogError(string.Format(MessagesResource.LogCommandNotDefined, name));
            }
        }

        internal static void RegisterObject(object obj)
        {
            if (obj == null)
            {
                JahroLogger.LogError(string.Format(MessagesResource.LogWatcherNullObjectError));
                return;
            }

            var members = RetrieveFieldsAndProperties(obj);
            foreach (var memberInfo in members)
            {
                var attribute = memberInfo.GetCustomAttributes(typeof(JahroWatchAttribute), false)[0] as JahroWatchAttribute;
                if (string.IsNullOrWhiteSpace(attribute.Name))
                {
                    attribute.Name = memberInfo.Name.TrimStart('_');
                }
                var entry = new ConsoleWatcherEntry(attribute.Name.Trim(), attribute.Description.Trim());
                entry.SetMemberInfo(memberInfo);
                entry.SetRuntimeObject(obj);
                Watcher.InitEntryToWatch(entry, attribute.GroupName.Trim());
            }

            var methods = RetrieveMethods(obj);
            foreach (var methodInfo in methods)
            {
                JahroCommandAttribute attribute = methodInfo.GetCustomAttributes(typeof(JahroCommandAttribute), false)[0] as JahroCommandAttribute;
                if (string.IsNullOrWhiteSpace(attribute.MethodName))
                {
                    attribute.MethodName = methodInfo.Name;
                }
                attribute.MethodName = attribute.MethodName.Replace(" ", "");
                var entry = new ConsoleCommandEntry(attribute.MethodName.Trim(), attribute.MethodDescription.Trim());
                entry.SetMethodInfo(methodInfo as MethodInfo);
                entry.SetReferenceObject(obj);
                entry.SetRuntime(true);
                entry.SetInvoker(new ConsoleCommandsInvoker.ObjectInvoker());
                Holder.InitCommandMethod(entry, attribute.GroupName.Trim());
            }
        }

        internal static void UnregisterObject(object obj)
        {
            if (obj == null)
            {
                JahroLogger.LogError(string.Format(MessagesResource.LogWatcherNullObjectError));
                return;
            }

            var members = RetrieveFieldsAndProperties(obj);
            foreach (var memberInfo in members)
            {
                var attribute = memberInfo.GetCustomAttributes(typeof(JahroWatchAttribute), false)[0] as JahroWatchAttribute;
                Watcher.RemoveEntryFromWatch(attribute, memberInfo);
            }
            var methods = RetrieveMethods(obj);
            foreach (var methodInfo in methods)
            {
                JahroCommandAttribute attribute = methodInfo.GetCustomAttributes(typeof(JahroCommandAttribute), false)[0] as JahroCommandAttribute;
                Holder.RemoveEntry(attribute, methodInfo as MethodInfo);
            }
        }

        internal static void RegisterCommand(string name, string description, string groupName, object obj, string methodName)
        {
            if (obj == null)
            {
                JahroLogger.LogError(string.Format(MessagesResource.LogCommandRegisterObjectNullReference, name));
                return;
            }
            Type objectType = obj.GetType();
            var memberInfo = objectType.GetMethod(methodName);
            if (memberInfo != null)
            {
                var entry = new ConsoleCommandEntry(name, description);
                entry.SetMethodInfo(memberInfo as MethodInfo);
                entry.SetReferenceObject(obj);
                entry.SetRuntime(true);
                entry.SetInvoker(new ConsoleCommandsInvoker.ObjectInvoker());
                Holder.InitCommandMethod(entry, groupName);
            }
            else
            {
                JahroLogger.LogError(string.Format(MessagesResource.LogCommandMethodNotFound, methodName, name));
            }
        }

        internal static void RegisterCallbackCommand(string name, string description, string groupName, Delegate del)
        {
            var entry = new ConsoleCommandEntry(name, description);
            entry.SetDelegateInfo(del);
            entry.SetRuntime(true);
            entry.SetInvoker(new ConsoleCommandsInvoker.CallbackInvoker());
            Holder.InitCommandMethod(entry, groupName);
        }

        internal static void UnregisterCommand(string name)
        {
            Holder.RemoveRuntimeEntry(name, string.Empty);
        }

        internal static void UnregisterCommand(string name, string groupName)
        {
            Holder.RemoveRuntimeEntry(name, groupName);
        }

        private static List<MemberInfo> RetrieveFieldsAndProperties(object obj)
        {
            List<MemberInfo> members = new List<MemberInfo>();
            var fieldsArray = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(m => m.IsStatic == false && m.IsDefined(typeof(JahroWatchAttribute), false));
            members.AddRange(fieldsArray);
            var propsArray = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                .Where(m => m.IsDefined(typeof(JahroWatchAttribute), false));
            members.AddRange(propsArray);
            return members;
        }

        private static List<MemberInfo> RetrieveMethods(object obj)
        {
            List<MemberInfo> members = new List<MemberInfo>();
            var methodsArray = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var methodInfo in methodsArray)
            {
                if (methodInfo.GetCustomAttribute<JahroCommandAttribute>() != null)
                {
                    members.Add(methodInfo);
                }
            }
            return members;
        }
    }
}
