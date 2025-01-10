using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;
using Jahro.Core.Snapshots;

namespace Jahro.View
{
    internal class SnapshotSessionsGroupLayout : MonoBehaviour
    {
        internal Dictionary<SnapshotSession, SnapshotSessionItem> SessionItems { get; private set; }

        [SerializeField]
        private GameObject snapshotItemPrefab;

        [SerializeField]
        private RectTransform ContentTransform;

        [SerializeField]
        private Image FoldoutImageOn;

        [SerializeField]
        private Image FoldoutImageOff;

        private Toggle _foldoutToggle;

        private Text _groupName;

        private ConsoleSnapshotsView _consoleSnapshotsView;

        public void Init(ConsoleSnapshotsView consoleSnapshotsView, bool isActiveGroup)
        {
            _consoleSnapshotsView = consoleSnapshotsView;

            SessionItems = new Dictionary<SnapshotSession, SnapshotSessionItem>();

            _groupName = this.GetComponentInChildren<Text>();
            _groupName.text = isActiveGroup ? "Current" : "Recent";

            _foldoutToggle = this.GetComponentInChildren<Toggle>();
            _foldoutToggle.SetIsOnWithoutNotify(true);

            _foldoutToggle.onValueChanged.AddListener(OnFoldoutStateChanged);
            OnFoldoutStateChanged(true);
        }

        public void AddSession(SnapshotSession session)
        {
            var item = CreateItem(session);
            if (session.GetStatus() == SnapshotSession.Status.Recording)
            {
                item.ShowHint();
            }
            SessionItems.Add(session, item);

        }

        public void DeleteSnapshot(SnapshotSession session)
        {
            if (SessionItems.ContainsKey(session))
            {
                var item = SessionItems[session];
                SessionItems.Remove(session);
                Destroy(item.gameObject);
            }
        }

        private void OnFoldoutStateChanged(bool state)
        {
            // Group.Foldout = state;
            FoldoutImageOn.gameObject.SetActive(state);
            FoldoutImageOff.gameObject.SetActive(!state);
            ContentTransform.gameObject.SetActive(state);
        }

        private SnapshotSessionItem CreateItem(SnapshotSession session)
        {
            var entryObject = GameObject.Instantiate(snapshotItemPrefab);
            var entryTransform = entryObject.GetComponent<RectTransform>();
            entryTransform.SetParent(ContentTransform);
            entryTransform.localScale = Vector3.one;
            var sessionEntry = entryObject.GetComponent<SnapshotSessionItem>();
            sessionEntry.Init(_consoleSnapshotsView, session);
            return sessionEntry;
        }
    }
}