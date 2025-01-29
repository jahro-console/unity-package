using System;

namespace JahroConsole.View
{
    public sealed class ConsoleEvents
    {
        public static ConsoleEvents Instance
        {
            get
            {
                if (_instance == null) _instance = new ConsoleEvents();
                return _instance;
            }
        }

        private static ConsoleEvents _instance;

        public event Action<ConsoleMainWindow.Mode> OnModeChanged;

        public event Action OnTextModeInputFocused;

        public event Action OnParametersWindowOpen;

        public event Action OnDetailsWindowOpen;

        public event Action<ConsoleMainWindow.Mode> OnConsoleOpen;

        public event Action OnConsoleClose;

        internal void ModeChanged(ConsoleMainWindow.Mode mode)
        {
            OnModeChanged?.Invoke(mode);
        }

        internal void TextModeInputFieldFocus()
        {
            OnTextModeInputFocused?.Invoke();
        }

        internal void OpenedParametersWindow()
        {
            OnParametersWindowOpen?.Invoke();
        }

        internal void OpenedDetailsWindow()
        {
            OnDetailsWindowOpen?.Invoke();
        }

        internal void ConsoleOpen(ConsoleMainWindow.Mode mode)
        {
            OnConsoleOpen?.Invoke(mode);
        }

        internal void ConsoleClose()
        {
            OnConsoleClose?.Invoke();
        }
    }
}