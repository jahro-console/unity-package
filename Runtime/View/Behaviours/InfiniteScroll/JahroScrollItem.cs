using System;
using JahroConsole.Core.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace JahroConsole.View
{
    internal class JahroScrollItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
    {
        private const float LONG_CLICK_TIME = 0.5f;

        private Color HoverMainAreaColor = new Color(1f, 1f, 1f, 0.03f);

        private Color HoverDetailsAreaColor = new Color(1f, 1f, 1f, 0.02f);

        [SerializeField]
        internal Text ConsoleCommandTextComponent;

        [SerializeField]
        internal Text ConsoleDetailsTextComponent;

        [SerializeField]
        private RectTransform ToggleAreaTrasform;

        [SerializeField]
        private GameObject ExpandToggle;

        [SerializeField]
        private GameObject ControlsRoot;

        [SerializeField]
        private GameObject MainAreaObject;

        [SerializeField]
        private GameObject DetailsAreaObject;

        [SerializeField]
        private Toggle SelectionToggle;

        private Image _mainAreaBackground;

        private Image _detailsAreaBackground;

        private bool _selectMode;

        private bool _longClicked;

        private bool _hovered;

        private bool selectable;

        private RectTransform CommandTextTransform;

        private RectTransform CommandDetailsTextTransform;

        private RectTransform MainAreaTransform;

        private RectTransform DetailsTextTransform;

        public event Action<JahroScrollItem, bool> OnExpand;

        public event Action<JahroScrollItem> OnCopy;

        public JahroLogEntity AssignedEntity { get; private set; }

        public JahroScrollView ParentScrollView { get; set; }

        public bool Expanded { get; private set; }

        private void Awake()
        {
            CommandTextTransform = ConsoleCommandTextComponent.GetComponent<RectTransform>();
            CommandDetailsTextTransform = ConsoleDetailsTextComponent.GetComponent<RectTransform>();

            _mainAreaBackground = MainAreaObject.GetComponent<Image>();
            _detailsAreaBackground = DetailsAreaObject.GetComponent<Image>();

            MainAreaTransform = MainAreaObject.GetComponent<RectTransform>();
            DetailsTextTransform = DetailsAreaObject.GetComponent<RectTransform>();
            DetailsAreaObject.SetActive(false);

            OnHoverMode(false);
            SelectMode(false);
        }

        public void SetUp(JahroLogEntity entity, string filter, bool selectMode)
        {
            AssignedEntity = entity;

            if (entity.LogType == EJahroLogType.JahroInfo)
            {
                selectable = false;
                ConsoleCommandTextComponent.text = entity.Message;
                ConsoleCommandTextComponent.resizeTextForBestFit = true;
                ConsoleCommandTextComponent.resizeTextMinSize = 8;
                ConsoleCommandTextComponent.resizeTextMaxSize = 12;
            }
            else
            {
                selectable = true;

                ConsoleCommandTextComponent.text = JahroLogsFormatter.FormatToConsoleMessage(entity, filter);
                ConsoleCommandTextComponent.resizeTextForBestFit = false;
            }

            RecalculateItemHeights();

            ExpandToggle.SetActive(AssignedEntity.HasDetails);
            ExpandMode(AssignedEntity.Expanded);

            _mainAreaBackground.color = _hovered ? HoverMainAreaColor : Color.clear;

            SelectMode(selectMode);

            if (AssignedEntity.Selected)
            {
                ForceSetSelection();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (ParentScrollView.MobileMode)
                return;

            OnHoverMode(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (ParentScrollView.MobileMode)
            {
                CancelInvoke("OnLongClick");
                return;
            }

            OnHoverMode(false);
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            if (ParentScrollView.MobileMode)
            {
                _longClicked = false;
                Invoke("OnLongClick", LONG_CLICK_TIME);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (ParentScrollView.MobileMode)
            {
                CancelInvoke("OnLongClick");
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_longClicked && ParentScrollView.MobileMode)
            {
                OnMainAreaClick();
            }
        }

        public void OnLongClick()
        {
            OnCopy?.Invoke(this);
        }

        public void OnMainAreaClick()
        {
            if (ParentScrollView.MobileMode && selectable && AssignedEntity.HasDetails)
            {
                ExpandMode(!Expanded);
                OnExpand?.Invoke(this, Expanded);
            }
        }

        public void OnExpandToggleValueChanged(bool isOn)
        {
            ExpandMode(isOn);
            OnExpand?.Invoke(this, isOn);
        }

        public void OnCopyButtonClick()
        {
            OnCopy?.Invoke(this);
        }

        public void OnSelectionChanged(bool isOn)
        {
            if (AssignedEntity != null)
            {
                AssignedEntity.Selected = isOn;
            }
        }

        public void ExpandMode(bool enabled)
        {
            Expanded = enabled;
            if (AssignedEntity != null) AssignedEntity.Expanded = Expanded;

            if (enabled)
            {
                ConsoleDetailsTextComponent.text = AssignedEntity.StackTrace;
            }

            RecalculateItemHeights();

            _mainAreaBackground.color = enabled ? HoverMainAreaColor : Color.clear;
            _detailsAreaBackground.color = enabled ? HoverDetailsAreaColor : Color.clear;
            DetailsAreaObject.SetActive(enabled);
            ExpandToggle.transform.GetChild(0).gameObject.SetActive(!enabled);
            ExpandToggle.transform.GetChild(1).gameObject.SetActive(enabled);
        }

        public void RecalculateItemHeights()
        {
            if (ParentScrollView == null) return;

            float contentWidth = ParentScrollView.ContentRect.rect.width;

            TextHeightCaluculator.Instance.UpdateReferenceSize(contentWidth);

            float mainHeight = TextHeightCaluculator.Instance.GetMainTextHeight(ConsoleCommandTextComponent.text);
            MainAreaTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, mainHeight);

            if (Expanded && AssignedEntity != null && AssignedEntity.HasDetails)
            {
                float detailsHeight = TextHeightCaluculator.Instance.GetDetailsTextHeight(ConsoleDetailsTextComponent.text);
                DetailsTextTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, detailsHeight);
                DetailsTextTransform.anchoredPosition = Vector2.down * MainAreaTransform.rect.height;
            }
            else
            {
                DetailsTextTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);
            }
        }

        public void SelectMode(bool enabled)
        {
            if (!selectable)
            {
                enabled = false;
            }

            SelectionToggle.isOn = AssignedEntity == null ? false : AssignedEntity.Selected;
            _selectMode = enabled;
            ToggleAreaTrasform.gameObject.SetActive(enabled);
            var offset = CommandTextTransform.offsetMin;
            offset.x = enabled ? 37 : 0;
            CommandTextTransform.offsetMin = offset;
            CommandDetailsTextTransform.offsetMin = Vector2.right * (enabled ? 37 : 5);
        }

        public bool IsSelected()
        {
            return AssignedEntity.Selected;
        }

        public void ForceSetSelection()
        {
            if (!selectable)
            {
                return;
            }

            SelectionToggle.isOn = AssignedEntity.Selected;
        }

        private void OnHoverMode(bool enabled)
        {
            if (!selectable)
            {
                ControlsRoot.SetActive(false);
                _mainAreaBackground.color = Color.clear;
                return;
            }
            _hovered = enabled;
            _mainAreaBackground.color = enabled ? HoverMainAreaColor : Color.clear;
            _detailsAreaBackground.color = enabled ? HoverDetailsAreaColor : Color.clear;
            if (!_selectMode)
            {
                ControlsRoot.SetActive(enabled);
            }
        }
    }
}