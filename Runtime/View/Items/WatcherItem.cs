using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Jahro.Core.Watcher;

namespace Jahro.View
{
    internal class WatcherItem : MonoBehaviour, IFlexGridItem
    {

        internal ConsoleWatcherEntry WatcherEntry { get; private set; }

        internal RectTransform RectTransform { get {return GetComponent<RectTransform>();}}

        [SerializeField]
        private Text _entryNameText;

        [SerializeField]
        private Text _entryValueText;

        private int _requiredSize;

        public Action OnClickedAction = delegate {};

        internal void Init(ConsoleWatcherEntry commandEntry)
        {
            WatcherEntry = commandEntry;

            SetName(WatcherEntry.Name);
            SetValue(WatcherEntry.GetShortStringValue(out _requiredSize));

            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        internal void UpdateValue()
        {
            SetValue(WatcherEntry.GetShortStringValue(out _requiredSize));
        }

        private void OnClicked()
        {
            OnClickedAction();
        }

        private void SetName(string name)
        {
            _entryNameText.text = name;
        }

        private void SetValue(string value)
        {
            _entryValueText.text = value;
        }

        public int GetRequeredSize()
        {
            return _requiredSize;
        }

        public RectTransform GetRectTransform()
        {
            return RectTransform;
        }
    }
}