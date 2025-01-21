using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Jahro.Core.Registry;
using Jahro.Core;
using Jahro.Core.Commands;
using Jahro.Core.Context;
using UnityEngine;
using UnityEngine.UI;

namespace Jahro.View
{
    internal class ConsoleVisualView : ConsoleBaseView
    {
        private const float NOTIFICATION_LIFETIME_SECONDS = 5f;

        public Transform ContentRoot;

        public GameObject GroupLayoutPrefab;

        public GameObject VisualCommandPrefab;

        public RectTransform ModalViewHolderTransform;

        public RectTransform Viewport;

        public GameObject CommandNotificationsHolder;

        public GameObject CommandNotificationsPrefab;

        public GameObject HintView;

        public ParamsModalView ParamsModalView { get; private set; }

        private ConsoleCommandHolder _commandsHolder;

        private VerticalLayoutGroup _contentLayoutGroup;

        private List<ConsoleGroupLayout> _groupLayouts;

        public void Awake()
        {
            _groupLayouts = new List<ConsoleGroupLayout>();
            _commandsHolder = ConsoleCommandsRegistry.Holder;
            _commandsHolder.OnGroupsChanged += OnGroupsChanged;
            _contentLayoutGroup = ContentRoot.GetComponent<VerticalLayoutGroup>();
            ParamsModalView = ModalViewHolderTransform.GetComponentInChildren<ParamsModalView>(true);
        }

        public void Start()
        {
            OnGroupsChanged();
        }

        public void CloseModalView()
        {
            if (ParamsModalView != null)
            {
                ParamsModalView.Close();
            }
        }

        public void CommandClicked(ConsoleVisualCommand visualCommand)
        {
            ParamsModalView.Open(visualCommand, MainWindow, Viewport, () => OnCommandSubmitted(visualCommand));
        }

        public void HintActionButtonClicked()
        {
            Application.OpenURL(JahroConfig.DocumentationCommandsOverview);
        }

        private void OnCommandSubmitted(ConsoleVisualCommand visualCommand)
        {
            StartCoroutine(CreateCommandNotification(visualCommand.CommandEntry));
        }

        protected override void OnActivate()
        {

        }

        protected override void OnDeactivate()
        {
            CloseModalView();
            ClearNotifications();
        }

        internal override void OnWindowRectChanged(Rect rect)
        {
            base.OnWindowRectChanged(rect);
            if (ParamsModalView != null)
            {
                ParamsModalView.Close();
            }
        }

        private void OnGroupsChanged()
        {
            foreach (var group in _commandsHolder.Groups)
            {
                if (_groupLayouts.Select(gl => gl.Group).Where(g => g == group).FirstOrDefault() == null)
                {
                    CreateGroup(group);
                }
            }

            List<ConsoleGroupLayout> layoutsToDestroy = new List<ConsoleGroupLayout>();
            foreach (var layoutGroup in _groupLayouts)
            {
                if (_commandsHolder.Groups.Contains(layoutGroup.Group) == false)
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

        private void ReorderGroups()
        {
            var orderedGroups = _groupLayouts.OrderBy(g => g.Group.Name);
            int order = 2;
            foreach (var groupLayout in orderedGroups)
            {
                if (groupLayout.Group is RecentGroup)
                {
                    groupLayout.transform.SetSiblingIndex(0);
                }
                else if (groupLayout.Group is FavoritesGroup<ConsoleCommandEntry>)
                {
                    groupLayout.transform.SetSiblingIndex(1);
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

        private ConsoleGroupLayout CreateGroup(SimpleGroup<ConsoleCommandEntry> group)
        {
            var groupLayoutObject = GameObject.Instantiate(GroupLayoutPrefab);
            var groupLayoutTransform = groupLayoutObject.GetComponent<RectTransform>();
            groupLayoutTransform.SetParent(ContentRoot);
            groupLayoutTransform.localScale = Vector3.one;
            var groupLayout = groupLayoutObject.GetComponent<ConsoleGroupLayout>();
            groupLayout.Init(group, VisualCommandPrefab, this);
            _groupLayouts.Add(groupLayout);
            return groupLayout;
        }

        private void DestroyGroup(ConsoleGroupLayout groupLayout)
        {
            GameObject.Destroy(groupLayout.gameObject);
        }

        private IEnumerator CreateCommandNotification(ConsoleCommandEntry entry)
        {
            var obj = GameObject.Instantiate(CommandNotificationsPrefab);
            obj.transform.SetParent(CommandNotificationsHolder.transform, false);
            obj.transform.SetAsLastSibling();
            var text = obj.GetComponentInChildren<Text>(true);
            text.text = "> " + entry.Name;

            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)CommandNotificationsHolder.transform);
            yield return new WaitForSeconds(NOTIFICATION_LIFETIME_SECONDS);

            GameObject.Destroy(obj);
        }

        private void ClearNotifications()
        {
            StopAllCoroutines();
            foreach (Transform child in CommandNotificationsHolder.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
}