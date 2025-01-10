using System;

namespace Jahro.Core.Commands
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class JahroCommandAttribute : Attribute
    {
        private string name;
        private readonly string description;
        private readonly string group;

        /// <summary>
        /// Marks a method as a console command for the Jahro.
        /// </summary>
        /// <param name="name">The unique identifier for the command. If omitted, the method name will be used by default.</param>
        /// <param name="group">The group the command belongs to. Defaults to "Default".</param>
        /// <param name="description">A brief description of the command. Displayed in Visual Mode for better context.</param>
        /// <example>
        /// Basic usage:
        /// <code>
        /// [JahroCommand("reset-game")]
        /// public void ResetGame() { }
        /// </code>
        /// With group and description:
        /// <code>
        /// [JahroCommand("save-game", "Game Management", "Saves the current game state")]
        /// public void SaveGame() { }
        /// </code>
        /// </example>
        public JahroCommandAttribute(string name = null, string group = "Default", string description = "")
        {
            this.name = name;
            this.group = string.IsNullOrWhiteSpace(group) ? "Default" : group.Trim();
            this.description = description;
        }

        /// <summary>
        /// Marks a method as a console command for the Jahro.
        /// </summary>
        /// <example>
        /// Basic usage:
        /// <code>
        /// [JahroCommand]
        /// public void ResetGame() { }
        /// </code>
        /// With group and description:
        /// <code>
        /// [JahroCommand("save-game", "Game Management", "Saves the current game state")]
        /// public void SaveGame() { }
        /// </code>
        /// </example>
        public JahroCommandAttribute() : this(null, "Default", "")
        {
        }

        /// <summary>
        /// Marks a method as a console command for the Jahro.
        /// </summary>
        /// <param name="name">The unique identifier for the command. If omitted, the method name will be used by default.</param>
        /// <example>
        /// Basic usage:
        /// <code>
        /// [JahroCommand("reset-game")]
        /// public void ResetGame() { }
        /// </code>
        /// With group and description:
        /// <code>
        /// [JahroCommand("save-game", "Game Management", "Saves the current game state")]
        /// public void SaveGame() { }
        /// </code>
        /// </example>
        public JahroCommandAttribute(string name) : this(name, "Default", "")
        {
        }

        /// <summary>
        /// Marks a method as a console command for the Jahro.
        /// </summary>
        /// <param name="name">The unique identifier for the command. If omitted, the method name will be used by default.</param>
        /// <param name="group">The group the command belongs to. Defaults to "Default".</param>
        /// <example>
        /// Basic usage:
        /// <code>
        /// [JahroCommand("reset-game", "Game Management")]
        /// public void ResetGame() { }
        /// </code>
        /// With group and description:
        /// <code>
        /// [JahroCommand("save-game", "Game Management", "Saves the current game state")]
        /// public void SaveGame() { }
        /// </code>
        /// </example>
        public JahroCommandAttribute(string name, string group) : this(name, group, "")
        {
        }

        internal string MethodName
        {
            get { return name; }
            set => name = value;
        }

        internal string MethodDescription
        {
            get { return description; }
        }

        internal string GroupName
        {
            get { return group; }
        }
    }
}