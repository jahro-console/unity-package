using System;

namespace JahroConsole
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public class JahroWatchAttribute : Attribute
    {

        private string name;
        private readonly string description;
        private readonly string group;

        /// <summary>
        /// Marks a variable (field or property) to be monitored in Jahro's Watcher Mode.
        /// </summary>
        /// <param name="name">The display name for the variable in Watcher Mode. Defaults to the variable's name if not provided.</param>
        /// <param name="group">The group the variable belongs to. Defaults to "Default".</param>
        /// <param name="description">A brief description of the variable. Useful for providing additional context in Watcher Mode.</param>
        /// <example>
        /// Basic usage:
        /// <code>
        /// [JahroWatch]
        /// private int _playerScore = 0;
        /// </code>
        /// With a custom name and group:
        /// <code>
        /// [JahroWatch("Player Health", "Player Stats")]
        /// private float _playerHealth = 100f;
        /// </code>
        /// With a custom description:
        /// <code>
        /// [JahroWatch("Gravity Force", "Physics", "The gravitational force applied to objects.")]
        /// public Vector3 Gravity => Physics.gravity;
        /// </code>
        /// </example>
        public JahroWatchAttribute(string name = null, string group = "Default", string description = "")
        {
            this.name = name;
            this.group = string.IsNullOrWhiteSpace(group) ? "Default" : group.Trim();
            this.description = description;
        }

        /// <summary>
        /// Marks a variable (field or property) to be monitored in Jahro's Watcher Mode.
        /// </summary>
        /// <example>
        /// Basic usage:
        /// <code>
        /// [JahroWatch]
        /// private int _playerScore = 0;
        /// </code>
        /// With a custom name and group:
        /// <code>
        /// [JahroWatch("Player Health", "Player Stats")]
        /// private float _playerHealth = 100f;
        /// </code>
        /// With a custom description:
        /// <code>
        /// [JahroWatch("Gravity Force", "Physics", "The gravitational force applied to objects.")]
        /// public Vector3 Gravity => Physics.gravity;
        /// </code>
        /// </example>
        public JahroWatchAttribute() : this(null, "Default", "")
        {
        }

        /// <summary>
        /// Marks a variable (field or property) to be monitored in Jahro's Watcher Mode.
        /// </summary>
        /// <param name="name">The display name for the variable in Watcher Mode. Defaults to the variable's name if not provided.</param>
        /// <example>
        /// Basic usage:
        /// <code>
        /// [JahroWatch("Player Score")]
        /// private int _playerScore = 0;
        /// </code>
        /// With a custom name and group:
        /// <code>
        /// [JahroWatch("Player Health", "Player Stats")]
        /// private float _playerHealth = 100f;
        /// </code>
        /// With a custom description:
        /// <code>
        /// [JahroWatch("Gravity Force", "Physics", "The gravitational force applied to objects.")]
        /// public Vector3 Gravity => Physics.gravity;
        /// </code>
        /// </example>        
        public JahroWatchAttribute(string name = null) : this(name, "Default", "")
        {
        }

        /// <summary>
        /// Marks a variable (field or property) to be monitored in Jahro's Watcher Mode.
        /// </summary>
        /// <param name="name">The display name for the variable in Watcher Mode. Defaults to the variable's name if not provided.</param>
        /// <param name="group">The group the variable belongs to. Defaults to "Default".</param>
        /// <example>
        /// Basic usage:
        /// <code>
        /// [JahroWatch("Player Score", "Player Stats")]
        /// private int _playerScore = 0;
        /// </code>
        /// With a custom name and group:
        /// <code>
        /// [JahroWatch("Player Health", "Player Stats")]
        /// private float _playerHealth = 100f;
        /// </code>
        /// With a custom description:
        /// <code>
        /// [JahroWatch("Gravity Force", "Physics", "The gravitational force applied to objects.")]
        /// public Vector3 Gravity => Physics.gravity;
        /// </code>
        /// </example>
        public JahroWatchAttribute(string name = null, string group = "Default") : this(name, group, "")
        {
        }

        internal string Name
        {
            get => name;
            set => name = value;
        }

        internal string Description => this.description;

        internal string GroupName => this.group;

    }
}