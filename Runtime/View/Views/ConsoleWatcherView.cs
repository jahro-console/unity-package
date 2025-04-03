using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JahroConsole.Core.Watcher;
using JahroConsole.Core.Registry;
using JahroConsole.Core.Context;

namespace JahroConsole.View
{
    internal class ConsoleWatcherView : ConsoleBaseView
    {
        public Transform ContentRoot;

        public GameObject GroupLayoutPrefab;

        public GameObject VisualCommandPrefab;

        public RectTransform ModalViewHolderTransform;

        public RectTransform Viewport;

        public GameObject HintView;

        public WatcherModalView WatcherModalView { get; private set; }

        private ConsoleWatcher _watcher;

        private VerticalLayoutGroup _contentLayoutGroup;

        private List<WatcherGroupLayout> _groupLayouts;

        public void Awake()
        {
            _groupLayouts = new List<WatcherGroupLayout>();
            _watcher = ConsoleCommandsRegistry.Watcher;
            _watcher.OnGroupsChanged += OnGroupsChanged;
            _contentLayoutGroup = ContentRoot.GetComponent<VerticalLayoutGroup>();
            WatcherModalView = ModalViewHolderTransform.GetComponentInChildren<WatcherModalView>(true);
        }

        public void Start()
        {
            OnGroupsChanged();
        }

        public void CloseModalView()
        {
            if (WatcherModalView != null)
            {
                WatcherModalView.Close();
            }
        }

        public void CommandClicked(WatcherItem item)
        {
            WatcherModalView.Open(item, MainWindow, Viewport);
        }

        public void HintActionButtonClicked()
        {
            Application.OpenURL(JahroConfig.DocumentationWatcherOverview);
        }

        protected override void OnActivate()
        {

        }

        protected override void OnDeactivate()
        {
            CloseModalView();
        }

        internal override void OnWindowRectChanged(Rect rect)
        {
            base.OnWindowRectChanged(rect);
            if (WatcherModalView != null)
            {
                WatcherModalView.Close();
            }
        }

        private void OnGroupsChanged()
        {
            foreach (var group in _watcher.Groups)
            {
                if (_groupLayouts.Select(gl => gl.Group).Where(g => g == group).FirstOrDefault() == null)
                {
                    CreateGroup(group);
                }
            }

            List<WatcherGroupLayout> layoutsToDestroy = new List<WatcherGroupLayout>();
            foreach (var layoutGroup in _groupLayouts)
            {
                if (_watcher.Groups.Contains(layoutGroup.Group) == false)
                {
                    layoutsToDestroy.Add(layoutGroup);
                    DestroyGroup(layoutGroup);
                }
            }
            foreach (var layout in layoutsToDestroy)
            {
                _groupLayouts.Remove(layout);
            }
            layoutsToDestroy.Clear();

            ReorderGroups();

            int totalItems = _groupLayouts.Sum(g => g.Group.Entries.Count);
            ShowHintView(totalItems == 0);
        }

        void OnDestroy()
        {
            _watcher.OnGroupsChanged -= OnGroupsChanged;
        }

        private void ReorderGroups()
        {
            var orderedGroups = _groupLayouts.OrderBy(g => g.Group.Name);
            int order = 1;
            foreach (var groupLayout in orderedGroups)
            {
                if (groupLayout.Group is FavoritesGroup<ConsoleWatcherEntry>)
                {
                    groupLayout.transform.SetSiblingIndex(0);
                }
                else
                {
                    groupLayout.transform.SetSiblingIndex(order);
                    order++;
                }
            }
        }

        private void ShowHintView(bool show)
        {
            HintView.SetActive(show);
            foreach (var groupLayout in _groupLayouts)
            {
                groupLayout.gameObject.SetActive(!show);
            }
        }

        private WatcherGroupLayout CreateGroup(SimpleGroup<ConsoleWatcherEntry> group)
        {
            var groupLayoutObject = GameObject.Instantiate(GroupLayoutPrefab);
            var groupLayoutTransform = groupLayoutObject.GetComponent<RectTransform>();
            groupLayoutTransform.SetParent(ContentRoot);
            groupLayoutTransform.localScale = Vector3.one;
            var groupLayout = groupLayoutObject.GetComponent<WatcherGroupLayout>();
            groupLayout.Init(group, VisualCommandPrefab, this);
            _groupLayouts.Add(groupLayout);
            return groupLayout;
        }

        private void DestroyGroup(WatcherGroupLayout groupLayout)
        {
            GameObject.Destroy(groupLayout.gameObject);
        }
    }
}