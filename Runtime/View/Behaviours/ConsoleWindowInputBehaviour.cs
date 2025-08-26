using System;
using System.Collections;
using System.Linq;
using JahroConsole.Core.Commands;
using JahroConsole.Core.Data;
using JahroConsole.Core.Registry;
using JahroConsole.View.InputHandling;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
	internal class ConsoleWindowInputBehaviour : MonoBehaviour
	{
		private AutocompliteHolder autocompliteHolder;

		private LayoutElement _layoutElement;

		private float _targetLayoutElementHeight;

		private CommandsQueue _commandsQueue;

		private TouchScreenKeyboard _keyboard;

		private string _previousAutocomliteText;

		private bool _enabled;

		private bool _previousFocused;

		public event Action<string> DidEnterCommand = delegate (string s) { };

		public InputField CommandsInputField;

		public Button CommandSubmitButton;

		private void Awake()
		{
			_layoutElement = GetComponent<LayoutElement>();

			CommandsInputField.onValueChanged.AddListener(OnInputChanged);
			CommandSubmitButton.onClick.AddListener(OnSubmitButtonClick);

			autocompliteHolder = GetComponentInChildren<AutocompliteHolder>();
			autocompliteHolder.OnAutocompliteCommandPress += OnAutocompliteCommandFill;
		}

		private void Start()
		{
			SetAutocomplitePanel(false);
			_commandsQueue = ConsoleStorageController.Instance.ConsoleStorage.CommandsQueue;
			_commandsQueue.ResetIndex();
			Enable();
			CommandsInputField.GetComponent<JahroInputField>().onSubmit += OnSubmitEvent;
		}

		private void Update()
		{
			if (!_enabled)
			{
				return;
			}

			if (autocompliteHolder.HasFocus)
			{
				if (JahroInput.GetKeyDown(KeyCode.Tab) && (JahroInput.GetKey(KeyCode.LeftShift) || JahroInput.GetKey(KeyCode.RightShift)))
				{
					autocompliteHolder.PreviousFocus();
				}
				else if (JahroInput.GetKeyDown(KeyCode.Tab))
				{
					autocompliteHolder.NextFocus();
				}
			}

			if (CommandsInputField.isFocused)
			{
				if (_previousFocused == false)
				{
					ConsoleEvents.Instance.TextModeInputFieldFocus();
				}

				if (JahroInput.GetKeyDown(KeyCode.UpArrow))
				{
					CommandsInputField.text = _commandsQueue.GetUp();
					CommandsInputField.caretPosition = CommandsInputField.text.Length;
				}
				else if (JahroInput.GetKeyDown(KeyCode.DownArrow))
				{
					CommandsInputField.text = _commandsQueue.GetDown();
					CommandsInputField.caretPosition = CommandsInputField.text.Length;
				}
				else if (JahroInput.GetKeyDown(KeyCode.Tab))
				{
					autocompliteHolder.SetFocus();
				}
			}

			_previousFocused = CommandsInputField.isFocused;
		}

		public void OnSubmitEvent(string text)
		{
			OnCommandEntered(CommandsInputField.text);
		}

		public bool IsFocused()
		{
			return CommandsInputField.isFocused;
		}

		public void Disable()
		{
			_enabled = false;
			CommandsInputField.text = "";
			SetAutocomplitePanel(false);
		}

		public void Enable()
		{
			_enabled = true;
		}

		private void OnSubmitButtonClick()
		{
			OnCommandEntered(CommandsInputField.text);
		}

		private void OnInputChanged(string input)
		{
			input = input.Trim();
			if (input.Length == 0)
			{
				autocompliteHolder.Clear();
				SetAutocomplitePanel(false);
				return;
			}
			if (input.Equals(_previousAutocomliteText))
			{
				return;
			}
			if (CommandsInputField.caretPosition != input.Length)
			{
				CommandsInputField.MoveTextEnd(true);
			}
			_previousAutocomliteText = input;
			var firstWord = input.Trim().Split(' ').First();
			var possibleEntries = ConsoleCommandsRegistry.Holder.GetPossibleCommandsNames(firstWord);

			autocompliteHolder.UpdateEntries(possibleEntries);
			if (possibleEntries.Count > 0)
			{
				SetAutocomplitePanel(true);
			}
		}

		private void OnAutocompliteCommandFill(ConsoleCommandEntry command)
		{
			_previousAutocomliteText = command.Name;
			autocompliteHolder.Clear();
			SetAutocomplitePanel(false);

			CommandsInputField.SetTextWithoutNotify(command.Name + " ");
			// CommandsInputField.text = commandText + " ";
			CommandsInputField.caretPosition = CommandsInputField.text.Length;
			if (!Application.isMobilePlatform)
			{
				CommandsInputField.ActivateInputField();
				StartCoroutine(ResetSelection());
			}
		}

		private void OnCommandEntered(string commandText)
		{
			if (string.IsNullOrEmpty(commandText.Trim()))
			{
				return;
			}

			string[] commandParameters;
			var command = ConsoleCommandValidator.SplitInput(commandText, out commandParameters);

			ConsoleCommandsRegistry.InvokeCommand(command, commandParameters);

			DidEnterCommand(commandText);
			_commandsQueue.PushCommand(commandText);

			CommandsInputField.text = "";
			if (!Application.isMobilePlatform)
			{
				CommandsInputField.Select();
				CommandsInputField.ActivateInputField();
			}
		}

		private IEnumerator ResetSelection()
		{
			yield return 0;
			CommandsInputField.MoveTextEnd(false);
		}

		private void SetAutocomplitePanel(bool visible)
		{
			if (visible)
			{
				_targetLayoutElementHeight = 78;
			}
			else
			{
				_targetLayoutElementHeight = 40;
			}

			_layoutElement.preferredHeight = _targetLayoutElementHeight;
		}
	}
}

