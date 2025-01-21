using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Jahro.Core.Snapshots;
using UnityEngine;
using UnityEngine.UI;

namespace Jahro.View
{
    internal class ConsoleSnapshotsView : ConsoleBaseView
    {
        [SerializeField]
        private GameObject _snapshotSessionGroupLayoutPrefab;

        [SerializeField]
        private RectTransform _contentRoot;

        [SerializeField]
        private DialogModalView _dialogModalView;

        private List<SnapshotSession> _snapshotSessions;

        private SnapshotSessionsGroupLayout _activeSessionGroupLayout;

        private SnapshotSessionsGroupLayout _inactiveSessionGroupLayout;

        public void Start()
        {
            InitSessions(SnapshotsManager.Instance.SnapshotSessions);

            SnapshotsManager.Instance.OnSessionAdded += OnSessionAdded;
            SnapshotsManager.Instance.OnSessionRemoved += OnSessionRemoved;
        }

        public void OpenDialog(string title, string description, Action onAction = null)
        {
            _dialogModalView.Open(MainWindow, _contentRoot, title, description, onAction);
        }

        public void CloseModalView()
        {
            _dialogModalView.Close();
        }

        private void InitSessions(List<SnapshotSession> sessions)
        {
            _snapshotSessions = sessions.OrderByDescending(s => s.GetRecordDate()).ToList();
            if (_activeSessionGroupLayout == null)
                _activeSessionGroupLayout = CreateGroup(true);
            if (_inactiveSessionGroupLayout == null)
                _inactiveSessionGroupLayout = CreateGroup(false);

            foreach (var session in _snapshotSessions)
            {
                if (session.GetStatus() == SnapshotSession.Status.Recording)
                {
                    _activeSessionGroupLayout.AddSession(session);
                }
                else
                {
                    _inactiveSessionGroupLayout.AddSession(session);
                }
            }

        }

        private void OnSessionRemoved(SnapshotSession session)
        {
            _inactiveSessionGroupLayout.DeleteSnapshot(session);
        }

        private void OnSessionAdded(SnapshotSession session)
        {
            if (session.GetStatus() == SnapshotSession.Status.Recording)
            {
                _activeSessionGroupLayout.AddSession(session);
            }
        }

        protected override void OnActivate()
        {

        }

        protected override void OnDeactivate()
        {
            CloseModalView();
        }

        internal override void SetTightMode(bool enabled)
        {
            base.SetTightMode(enabled);
            if (_activeSessionGroupLayout != null)
            {
                foreach (var item in _activeSessionGroupLayout.SessionItems.Values)
                {
                    item.SetUITightMode(enabled);
                }
            }
            if (_inactiveSessionGroupLayout != null)
            {
                foreach (var item in _inactiveSessionGroupLayout.SessionItems.Values)
                {
                    item.SetUITightMode(enabled);
                }
            }
        }

        internal override void OnWindowRectChanged(Rect rect)
        {
            base.OnWindowRectChanged(rect);
            CloseModalView();
        }

        private SnapshotSessionsGroupLayout CreateGroup(bool isActiveGroup)
        {
            var obj = GameObject.Instantiate(_snapshotSessionGroupLayoutPrefab);
            var groupLayoutTransform = obj.GetComponent<RectTransform>();
            groupLayoutTransform.SetParent(_contentRoot);
            groupLayoutTransform.localScale = Vector3.one;
            var groupLayout = obj.GetComponent<SnapshotSessionsGroupLayout>();
            groupLayout.Init(this, isActiveGroup);
            return groupLayout;
        }

    }
}