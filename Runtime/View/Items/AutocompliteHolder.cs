using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Jahro.Core.Commands;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Jahro.View
{
    internal class AutocompliteHolder : MonoBehaviour
    {
        internal bool HasFocus {get {return _currentSelectable != null;}}

        private List<AutocompliteCommandEntry> _entriesObjects = new List<AutocompliteCommandEntry>();

        [SerializeField]
        private GameObject AutocompliteEntryPrefab;

        [SerializeField]
        private RectTransform HolderTransform;

        private Selectable _currentSelectable;

        private int _currentEntriesCount;

        public event Action<ConsoleCommandEntry> OnAutocompliteCommandPress = delegate {};

        internal void UpdateEntries(List<ConsoleCommandEntry> entries)
        {
            _currentEntriesCount = entries.Count;
            if (entries.Count > _entriesObjects.Count)
            {
                for (int i=_entriesObjects.Count; i < entries.Count; i++)
                {
                    _entriesObjects.Add(CreateVisualCommandEntry());
                }
            }

            for (int i=0; i<_entriesObjects.Count; i++)
            {
                if (i < entries.Count)
                {
                    _entriesObjects[i].Show(entries[i]);
                }
                else
                {
                    _entriesObjects[i].Hide();
                }
            }
        }

        internal void SetFocus()
        {
            SetFocusOnFirst();
        }

        internal void NextFocus()
        {
            if (_currentEntriesCount == 1)
            {
                OnAutocompliteCommandPress(_entriesObjects[0].CurrentCommand);
                _currentSelectable = null;
            }

            if (_currentSelectable != null && EventSystem.current.currentSelectedGameObject == _currentSelectable.gameObject)
            {
                var nextSelectable = _currentSelectable.FindSelectableOnRight();
                if (nextSelectable != null)
                {
                    if (nextSelectable.GetComponent<AutocompliteCommandEntry>() != null)
                    {
                        nextSelectable.Select();
                        _currentSelectable = nextSelectable;
                    }
                    else
                    {
                        SetFocusOnFirst();
                    }
                }
            }
            else
            {
                _currentSelectable = null;
            }
        }

        internal void PreviousFocus()
        {
            if (_currentSelectable != null && EventSystem.current.currentSelectedGameObject == _currentSelectable.gameObject)
            {
                var nextSelectable = _currentSelectable.FindSelectableOnLeft();
                if (nextSelectable != null)
                {
                    nextSelectable.Select();
                    _currentSelectable = nextSelectable;
                }
            }
            else
            {
                _currentSelectable = null;
            }
        }

        private void SetFocusOnFirst()
        {
            if (_entriesObjects.Count > 0 && _entriesObjects[0].isActiveAndEnabled)
            {
                _currentSelectable = _entriesObjects[0].GetComponent<Selectable>();
                _currentSelectable.Select();
            }
            else
            {
                _currentSelectable = null;
            }
        }

        public void Clear()
        {
            foreach(var entry in _entriesObjects)
            {
                entry.Hide();
            }
            _currentSelectable = null;
        }

        private AutocompliteCommandEntry CreateVisualCommandEntry()
        {
            var entryObject = GameObject.Instantiate(AutocompliteEntryPrefab);
            var entryTransform = entryObject.GetComponent<RectTransform>();
            entryTransform.SetParent(HolderTransform);
            entryTransform.localScale = Vector3.one;
            var entry = entryObject.GetComponent<AutocompliteCommandEntry>();
            entry.Init(OnAutocompliteCommandPress);
            entry.Hide();
            return entry;
        }
    }
}