using System.Collections;
using System.Collections.Generic;
using Jahro.Core.Data;
using Jahro;
using Jahro.Core.Context;
using UnityEngine;

namespace Jahro.View
{
    internal class ConsoleBaseView : MonoBehaviour
    {
        public ViewToggleItem ToggleItem;

        public BaseOptionsView OptionsView;

        public ConsoleMainWindow MainWindow { get; private set; }

        protected JahroContext Context;

        public virtual void InitView(ConsoleMainWindow mainWindow)
        {
            MainWindow = mainWindow;
        }

        public virtual void InitContext(JahroContext context)
        {
            Context = context;
        }

        public void Activate()
        {
            gameObject.SetActive(true);
            OnActivate();
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
            OnDeactivate();
        }


        public virtual void OnStateSave(ConsoleStorage storage)
        {

        }

        public virtual void OnStateLoad(ConsoleStorage storage)
        {

        }

        protected virtual void OnActivate()
        {

        }

        protected virtual void OnDeactivate()
        {
            OptionsView?.Close();
        }


        internal virtual void ShowOptions()
        {
            OptionsView?.Show();
        }

        internal virtual void CloseOptions()
        {
            OptionsView?.Close();
        }

        internal virtual void SetTightMode(bool enabled)
        {
            ToggleItem?.SetTightMode(enabled);
        }

        internal virtual void OnWindowRectChanged(Rect rect)
        {

        }
    }
}