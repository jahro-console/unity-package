using JahroConsole.Core.Registry;

namespace JahroConsole
{
    public static partial class Jahro
    {
        /// <summary>
        /// Registers an object with Jahro to allow monitoring of the object's fields marked with the "JahroWatch" and "JahroCommands" attributes. 
        /// </summary>
        /// <param name="obj">Object with fields, properties and methods marked by the "Jahro" attributes to be registered for monitoring.</param>
        public static void RegisterObject(object obj)
        {
            if (!Enabled)
            {
                return;
            }
            ConsoleCommandsRegistry.RegisterObject(obj);
        }

        /// <summary>
        /// Unregisters an object from Jahro, stopping monitoring of the object's fields, properties and methods marked with the "Jahro" attributes. 
        /// </summary>
        /// <param name="obj">Object with fields marked by the "Jahro" attributes to be unregistered from monitoring.</param>
        public static void UnregisterObject(object obj)
        {
            if (!Enabled)
            {
                return;
            }
            ConsoleCommandsRegistry.UnregisterObject(obj);
        }
    }
}