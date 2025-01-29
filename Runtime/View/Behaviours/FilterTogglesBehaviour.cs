using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class FilterTogglesBehaviour : MonoBehaviour
    {
        public ConsoleWindowOutputBehaviour Output;

        public Toggle ToggleDebug;

        public Toggle ToggleWarnings;

        public Toggle ToggleErrors;

        public Toggle ToggleCommands;

        private void Awake()
        {
            var toggles = GetComponentsInChildren<Toggle>();

            foreach (var toggle in toggles)
            {
                toggle.onValueChanged.AddListener(delegate (bool arg0)
                {
                    Output.SetFilter(ToggleDebug.isOn, ToggleWarnings.isOn, ToggleErrors.isOn, ToggleCommands.isOn);
                });
            }
        }

        public void UpdateStates(bool debug, bool warnings, bool errors, bool commands)
        {
            ToggleDebug.SetIsOnWithoutNotify(debug);
            ToggleWarnings.SetIsOnWithoutNotify(warnings);
            ToggleErrors.SetIsOnWithoutNotify(errors);
            ToggleCommands.SetIsOnWithoutNotify(commands);
            Output.SetFilter(debug, warnings, errors, commands);
        }
    }
}