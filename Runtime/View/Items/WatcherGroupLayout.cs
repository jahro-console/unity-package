using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using JahroConsole.Core.Watcher;
using JahroConsole.Core.Registry;

namespace JahroConsole.View
{
    internal class WatcherGroupLayout : MonoBehaviour, IFlexGridLayout
    {

        private static int PREFERED_WIDTH = 100;

        private static int PREFERED_HEIGHT = 50;

        internal SimpleGroup<ConsoleWatcherEntry> Group { get; private set; }

        public RectTransform ContentTransform;

        public Image FoldoutImageOn;

        public Image FoldoutImageOff;

        private Toggle _foldoutToggle;

        private Text _groupName;

        private Dictionary<ConsoleWatcherEntry, WatcherItem> _watcherItems;

        private GameObject _visualEntryPrefab;

        private ConsoleWatcherView _watcherView;

        private FlexGrid _grid;

        internal void Init(SimpleGroup<ConsoleWatcherEntry> group, GameObject visualEntryPrefab, ConsoleWatcherView watcherView)
        {
            Group = group;
            _watcherView = watcherView;
            _visualEntryPrefab = visualEntryPrefab;
            _grid = this.GetComponentInChildren<FlexGrid>();
            _grid.Init(this);
            _groupName = this.GetComponentInChildren<Text>();
            _groupName.text = Group.Name;

            _foldoutToggle = this.GetComponentInChildren<Toggle>();
            _foldoutToggle.SetIsOnWithoutNotify(group.Foldout);

            _foldoutToggle.onValueChanged.AddListener(OnFoldoutStateChanged);
            OnFoldoutStateChanged(group.Foldout);

            Group.OnEntriesChanged += OnEntriesChanged;

            _watcherItems = new Dictionary<ConsoleWatcherEntry, WatcherItem>();
            foreach (var entry in group.Entries)
            {
                if (_watcherItems.ContainsKey(entry) == false)
                {
                    _watcherItems.Add(entry, CreateVisualEntry(entry, visualEntryPrefab));
                }
            }

            UpdateGroupCounter(_watcherItems.Count);
        }

        private void OnFoldoutStateChanged(bool state)
        {
            Group.Foldout = state;
            FoldoutImageOn.gameObject.SetActive(state);
            FoldoutImageOff.gameObject.SetActive(!state);
            ContentTransform.gameObject.SetActive(state);
        }

        private void Update()
        {
            if (!ContentTransform.gameObject.activeInHierarchy)
            {
                return;
            }

            if (_watcherItems != null)
            {
                foreach (var item in _watcherItems.Values)
                {
                    item.UpdateValue();
                }
            }
        }

        private void OnEntriesChanged()
        {
            //Adding commands that isn't in group yet
            foreach (var entry in Group.Entries)
            {
                if (_watcherItems.ContainsKey(entry) == false)
                {
                    _watcherItems.Add(entry, CreateVisualEntry(entry, _visualEntryPrefab));
                }
            }
            //Remove visual entries which is no longer in group
            List<ConsoleWatcherEntry> entriesToRemove = new List<ConsoleWatcherEntry>();
            foreach (var visualEntry in _watcherItems.Keys)
            {
                if (Group.Entries.Contains(visualEntry) == false)
                {
                    RemoveEntry(visualEntry);
                    entriesToRemove.Add(visualEntry);
                }
            }

            if (_watcherView.WatcherModalView.IsOpen()
                && entriesToRemove.Contains(_watcherView.WatcherModalView.CurrentWatcherEntry))
            {
                var visualCommand = _watcherItems[_watcherView.WatcherModalView.CurrentWatcherEntry];
                if (visualCommand == _watcherView.WatcherModalView.CurrentWatcherItem)
                {
                    _watcherView.CloseModalView();
                }
            }
            foreach (var entry in entriesToRemove)
            {
                _watcherItems.Remove(entry);
            }
            entriesToRemove.Clear();

            UpdateGroupCounter(_watcherItems.Count);
            // _grid.CalculateLayoutInputVertical();
            // LayoutRebuilder.MarkLayoutForRebuild(ContentTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(ContentTransform);
        }

        private void UpdateGroupCounter(int count)
        {
            _groupName.text = string.Format("{0} ({1})", Group.Name, count);
        }

        private WatcherItem CreateVisualEntry(ConsoleWatcherEntry entry, GameObject visualEntryPrefab)
        {
            var entryObject = GameObject.Instantiate(visualEntryPrefab);
            var entryTransform = entryObject.GetComponent<RectTransform>();
            entryTransform.SetParent(ContentTransform);
            entryTransform.localScale = Vector3.one;
            var visualEntry = entryObject.GetComponent<WatcherItem>();
            visualEntry.Init(entry);
            visualEntry.OnClickedAction += delegate
            {
                _watcherView.CommandClicked(visualEntry);
            };
            return visualEntry;
        }

        private void RemoveEntry(ConsoleWatcherEntry entry)
        {
            var visualCommand = _watcherItems[entry];
            if (visualCommand != null && visualCommand.gameObject != null)
            {
                GameObject.Destroy(visualCommand.gameObject);
            }
        }

        public IEnumerable<IFlexGridItem> GetOrderedItems()
        {
            return _watcherItems.Values.OrderByDescending(i => i.GetRequeredSize()).ThenBy(i => i.WatcherEntry.Name);
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