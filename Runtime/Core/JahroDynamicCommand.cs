using System;
using JahroConsole.Core.Registry;

namespace JahroConsole
{
    /// <summary>
    /// Main API File
    /// </summary>
    public static partial class Jahro
    {
        /// <summary>
        /// Registers a new console command using the given name and the method on the provided object.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="obj">The object containing the method to be invoked.</param>
        /// <param name="methodName">The name of the method to be invoked.</param>
        public static void RegisterCommand(string name, object obj, string methodName)
        {
            RegisterCommand(name, string.Empty, string.Empty, obj, methodName);
        }

        /// <summary>
        /// Registers a new console command with a description, using the given name and the method on the provided object.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="obj">The object containing the method to be invoked.</param>
        /// <param name="methodName">The name of the method to be invoked.</param>
        public static void RegisterCommand(string name, string description, object obj, string methodName)
        {
            RegisterCommand(name, description, string.Empty, obj, methodName);
        }

        /// <summary>
        /// Registers a new console command with a description and group name, using the given name and the method on the provided object.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="groupName">The group name of the command.</param>
        /// <param name="obj">The object containing the method to be invoked.</param>
        /// <param name="methodName">The name of the method to be invoked.</param>
        public static void RegisterCommand(string name, string description, string groupName, object obj, string methodName)
        {
            if (!Enabled)
            {
                return;
            }
            ConsoleCommandsRegistry.RegisterCommand(name, description, groupName, obj, methodName);
        }

        /// <summary>
        /// Registers a new console command with the given name and callback action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="callback">The action to be invoked when the command is called.</param>
        public static void RegisterCommand(string name, Action callback)
        {
            RegisterCommand(name, string.Empty, string.Empty, callback);
        }

        /// <summary>
        /// Registers a new console command with a description, using the given name and callback action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="callback">The action to be invoked when the command is called.</param>
        public static void RegisterCommand(string name, string description, Action callback)
        {
            RegisterCommand(name, description, string.Empty, callback);
        }

        /// <summary>
        /// Registers a new console command with a description and group name, using the given name and callback action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="groupName">The group name of the command.</param>
        /// <param name="callback">The action to be invoked when the command is called.</param>
        public static void RegisterCommand(string name, string description, string groupName, Action callback)
        {
            if (!Enabled)
            {
                return;
            }
            ConsoleCommandsRegistry.RegisterCallbackCommand(name, description, groupName, callback);
        }

        /// <summary>
        /// Registers a new console command with a parameter of type T, using the given name and callback action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="callback">The action to be invoked when the command is called, accepting a parameter of type T.</param>
        public static void RegisterCommand<T>(string name, Action<T> callback)
        {
            RegisterCommand(name, string.Empty, string.Empty, callback);
        }

        /// <summary>
        /// Registers a new console command with a description, that has a parameter of type T, using the given name and callback action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="callback">The action to be invoked when the command is called, accepting a parameter of type T.</param>
        public static void RegisterCommand<T>(string name, string description, Action<T> callback)
        {
            RegisterCommand(name, description, string.Empty, callback);
        }

        /// <summary>
        /// Registers a new console command with a description and group name, that has a parameter of type T, using the given name and callback action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="groupName">The group name of the command.</param>
        /// <param name="callback">The action to be invoked when the command is called, accepting a parameter of type T.</param>
        public static void RegisterCommand<T>(string name, string description, string groupName, Action<T> callback)
        {
            if (!Enabled)
            {
                return;
            }
            ConsoleCommandsRegistry.RegisterCallbackCommand(name, description, groupName, callback);
        }

        /// <summary>
        /// Registers a new console command that takes two parameters of types T1 and T2, using the given name and callback action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="callback">The action to be invoked when the command is called, accepting two parameters of types T1 and T2.</param>
        public static void RegisterCommand<T1, T2>(string name, Action<T1, T2> callback)
        {
            RegisterCommand(name, string.Empty, string.Empty, callback);
        }

        /// <summary>
        /// Registers a new console command with a description, that takes two parameters of types T1 and T2, using the given name and callback action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="callback">The action to be invoked when the command is called, accepting two parameters of types T1 and T2.</param>
        public static void RegisterCommand<T1, T2>(string name, string description, Action<T1, T2> callback)
        {
            RegisterCommand(name, description, string.Empty, callback);
        }

        /// <summary>
        /// Registers a new console command with a description and group name, that takes two parameters of types T1 and T2, using the given name and callback action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="groupName">The group name of the command.</param>
        /// <param name="callback">The action to be invoked when the command is called, accepting two parameters of types T1 and T2.</param>
        public static void RegisterCommand<T1, T2>(string name, string description, string groupName, Action<T1, T2> callback)
        {
            if (!Enabled)
            {
                return;
            }
            ConsoleCommandsRegistry.RegisterCallbackCommand(name, description, groupName, callback);
        }

        /// <summary>
        /// Registers a new console command that takes three parameters of types T1, T2, and T3, using the given name and callback action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="callback">The action to be invoked when the command is called, accepting three parameters of types T1, T2, and T3.</param>
        public static void RegisterCommand<T1, T2, T3>(string name, Action<T1, T2, T3> callback)
        {
            RegisterCommand(name, string.Empty, string.Empty, callback);
        }

        /// <summary>
        /// Registers a new console command with a description, that takes three parameters of types T1, T2, and T3, using the given name and callback action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="callback">The action to be invoked when the command is called, accepting three parameters of types T1, T2, and T3.</param>
        public static void RegisterCommand<T1, T2, T3>(string name, string description, Action<T1, T2, T3> callback)
        {
            RegisterCommand(name, description, string.Empty, callback);
        }

        /// <summary>
        /// Registers a new console command with a description and group name, that takes three parameters of types T1, T2, and T3, using the given name and callback action.
        /// </summary>
        /// <param name="name">The name of the command.</param>
        /// <param name="description">The description of the command.</param>
        /// <param name="groupName">The group name of the command.</param>
        /// <param name="callback">The action to be invoked when the command is called, accepting three parameters of types T1, T2, and T3.</param>
        public static void RegisterCommand<T1, T2, T3>(string name, string description, string groupName, Action<T1, T2, T3> callback)
        {
            if (!Enabled)
            {
                return;
            }
            ConsoleCommandsRegistry.RegisterCallbackCommand(name, description, groupName, callback);
        }

        /// <summary>
        /// Unregisters a previously registered console command using the given name.
        /// </summary>
        /// <param name="name">The name of the command to unregister.</param>
        public static void UnregisterCommand(string name)
        {
            if (!Enabled)
            {
                return;
            }
            ConsoleCommandsRegistry.UnregisterCommand(name);
        }

        /// <summary>
        /// Unregisters a previously registered console command using the given name and group name.
        /// </summary>
        /// <param name="name">The name of the command to unregister.</param>
        /// <param name="groupName">The group name of the command to unregister.</param>
        public static void UnregisterCommand(string name, string groupName)
        {
            if (!Enabled)
            {
                return;
            }
            ConsoleCommandsRegistry.UnregisterCommand(name, groupName);
        }
    }
}