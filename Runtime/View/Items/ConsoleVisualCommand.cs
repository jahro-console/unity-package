using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Jahro.Core.Commands;

namespace Jahro.View
{
    internal class ConsoleVisualCommand : MonoBehaviour, IFlexGridItem
    {
        internal ConsoleCommandEntry CommandEntry { get; private set; }

        private Text _nameText;

        public Action OnClickedAction = delegate {};

        internal void Init(ConsoleCommandEntry commandEntry)
        {
            CommandEntry = commandEntry;

            _nameText = GetComponentInChildren<Text>();
            SetName(CommandEntry.Name);

            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            OnClickedAction();
        }

        private void SetName(string name)
        {
            _nameText.text = name;
        }

        public int GetRequeredSize()
        {
            if (_nameText.text.Length > 13)
            {
                return 2;
            }
            else
            {
                return 1;
            }
        }

        public RectTransform GetRectTransform()
        {
            return GetComponent<RectTransform>();
        }
    }
}