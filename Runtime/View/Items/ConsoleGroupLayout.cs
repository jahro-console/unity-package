using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using JahroConsole.Core.Commands;
using JahroConsole.Core.Registry;

namespace JahroConsole.View
{
    internal class ConsoleGroupLayout : MonoBehaviour, IFlexGridLayout
    {

        private static int PREFERED_WIDTH = 100;

        private static int PREFERED_HEIGHT = 40;

        internal SimpleGroup<ConsoleCommandEntry> Group { get; private set; }

        public RectTransform ContentTransform;

        public Image FoldoutImageOn;

        public Image FoldoutImageOff;

        private Toggle _foldoutToggle;

        private Text _groupName;

        private FlexGrid _grid;

        private Dictionary<ConsoleCommandEntry, ConsoleVisualCommand> _visualCommands;

        private GameObject _visualEntryPrefab;

        private ConsoleVisualView _visualView;

        internal void Init(SimpleGroup<ConsoleCommandEntry> group, GameObject visualEntryPrefab, ConsoleVisualView visualView)
        {
            Group = group;
            _visualView = visualView;
            _visualEntryPrefab = visualEntryPrefab;

            _grid = this.GetComponentInChildren<FlexGrid>();
            _grid.Init(this);
            // _grid.NativeOrder = group.LIFO;
            _groupName = this.GetComponentInChildren<Text>();
            _groupName.text = Group.Name;

            _foldoutToggle = this.GetComponentInChildren<Toggle>();
            _foldoutToggle.SetIsOnWithoutNotify(group.Foldout);
            _foldoutToggle.onValueChanged.AddListener(OnFoldoutStateChanged);
            OnFoldoutStateChanged(group.Foldout);

            Group.OnEntriesChanged += OnEntriesChanged;

            _visualCommands = new Dictionary<ConsoleCommandEntry, ConsoleVisualCommand>();
            foreach (var entry in group.Entries)
            {
                if (_visualCommands.ContainsKey(entry) == false)
                {
                    _visualCommands.Add(entry, CreateVisualEntry(entry, visualEntryPrefab));
                }
            }
            UpdateGroupCounter(_visualCommands.Count);
        }

        void OnDestroy()
        {
            Group.OnEntriesChanged -= OnEntriesChanged;
        }

        private void OnFoldoutStateChanged(bool state)
        {
            Group.Foldout = state;
            FoldoutImageOn.gameObject.SetActive(state);
            FoldoutImageOff.gameObject.SetActive(!state);
            ContentTransform.gameObject.SetActive(state);
        }

        private void OnEntriesChanged()
        {
            //Adding commands that isn't in group yet
            foreach (var entry in Group.Entries)
            {
                if (_visualCommands.ContainsKey(entry) == false)
                {
                    _visualCommands.Add(entry, CreateVisualEntry(entry, _visualEntryPrefab));
                }
            }
            //Remove visual entries which is no longer in group
            List<ConsoleCommandEntry> entriesToRemove = new List<ConsoleCommandEntry>();
            foreach (var visualEntry in _visualCommands.Keys)
            {
                if (Group.Entries.Contains(visualEntry) == false)
                {
                    RemoveEntry(visualEntry);
                    entriesToRemove.Add(visualEntry);
                }
            }

            if (_visualView.ParamsModalView != null && _visualView.ParamsModalView.IsOpen()
                && entriesToRemove.Contains(_visualView.ParamsModalView.CurrentCommandEntry))
            {
                var visualCommand = _visualCommands[_visualView.ParamsModalView.CurrentCommandEntry];
                if (visualCommand == _visualView.ParamsModalView.CurrentVisualCommand)
                {
                    _visualView.CloseModalView();
                }
            }
            foreach (var entry in entriesToRemove)
            {
                _visualCommands.Remove(entry);
            }
            entriesToRemove.Clear();

            UpdateGroupCounter(_visualCommands.Count);
            _grid.CalculateLayoutInputVertical();
        }

        private void UpdateGroupCounter(int count)
        {
            _groupName.text = string.Format("{0} ({1})", Group.Name, count);
        }

        private ConsoleVisualCommand CreateVisualEntry(ConsoleCommandEntry entry, GameObject visualEntryPrefab)
        {
            var entryObject = GameObject.Instantiate(visualEntryPrefab);
            var entryTransform = entryObject.GetComponent<RectTransform>();
            entryTransform.SetParent(ContentTransform);
            entryTransform.localScale = Vector3.one;
            var visualEntry = entryObject.GetComponent<ConsoleVisualCommand>();
            visualEntry.Init(entry);
            visualEntry.OnClickedAction += delegate
            {
                _visualView.CommandClicked(visualEntry);
            };
            return visualEntry;
        }

        private void RemoveEntry(ConsoleCommandEntry entry)
        {
            var visualCommand = _visualCommands[entry];
            if (visualCommand != null && visualCommand.gameObject != null)
            {
                GameObject.Destroy(visualCommand.gameObject);
            }
        }

        public IEnumerable<IFlexGridItem> GetOrderedItems()
        {
            if (Group is RecentGroup)
            {
                return _visualCommands.Values.OrderByDescending(i => i.CommandEntry.LastExecuted);//.ThenBy(i => i.CommandEntry.Name);
            }
            return _visualCommands.Values.OrderBy(i => i.CommandEntry.Name);
        }

        public float GetPreferedWidth()
        {
            float contentWidth = ContentTransform.rect.width;
            int preferedCount = Mathf.CeilToInt(contentWidth / (PREFERED_WIDTH + FlexGrid.Spacing.x));
            return Mathf.RoundToInt((contentWidth - FlexGrid.Spacing.x * (preferedCount + 1)) / preferedCount);
        }

        public float GetPreferedHeight()
        {
            return PREFERED_HEIGHT;
        }
    }
}