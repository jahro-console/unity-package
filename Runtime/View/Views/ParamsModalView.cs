using System.Collections.Generic;
using Jahro.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System;
using Jahro.Core.Registry;
using Jahro.Core.Commands;
using Jahro.Core.Logging;
using Jahro.Logging;

namespace Jahro.View
{
    internal class ParamsModalView : MonoBehaviour, IPointerClickHandler
    {

        public RectTransform ModalViewTransform;

        public Text TitleText;

        public Text DescriptionText;

        public Button CloseButton;

        public Button SubmitButton;

        public Toggle FavoritesToggle;

        public ScrollRect DynamicScrollContent;

        public RectTransform ParamsContentHolder;

        public GameObject StringParamPrefab;

        public GameObject IntParamPrefab;

        public GameObject BoolParamPrefab;

        public GameObject FloatParamPrefab;

        public GameObject DoubleParamPrefab;

        public GameObject ArrayParamPrefab;

        public GameObject Vector3ParamPrefab;

        public GameObject Vector2ParamPrefab;

        public GameObject EnumParamPrefab;

        internal ConsoleCommandEntry CurrentCommandEntry { get; private set; }

        public ConsoleVisualCommand CurrentVisualCommand { get; private set; }

        private List<BaseVisualParam> _visualParams = new List<BaseVisualParam>();

        private RectTransform _parentRect;

        private LayoutElement _dynamicContentHolderLayout;

        private float _keyboardHeightPlace;

        private bool _subscribed;

        private Action _currentCommandOnSubmit;

        private void Awake()
        {
            _dynamicContentHolderLayout = DynamicScrollContent.GetComponent<LayoutElement>();

            CloseButton.onClick.AddListener(OnCloseClick);
            SubmitButton.onClick.AddListener(OnSubmitClick);

            FavoritesToggle.onValueChanged.AddListener(OnFavoritesStateChanged);
        }

        private void Start()
        {
            // gameObject.SetActive(false);
        }

        public void Open(ConsoleVisualCommand visualCommand, ConsoleMainWindow mainWindow, RectTransform parentRect, Action onSubmitted = null)
        {
            if (IsOpen())
            {
                return;
            }

            _parentRect = parentRect;
            CurrentVisualCommand = visualCommand;
            CurrentCommandEntry = visualCommand.CommandEntry;
            _currentCommandOnSubmit = onSubmitted;

            TitleText.text = visualCommand.CommandEntry.Name;
            if (string.IsNullOrEmpty(visualCommand.CommandEntry.Description))
            {
                DescriptionText.gameObject.SetActive(false);
            }
            else
            {
                DescriptionText.gameObject.SetActive(true);
                DescriptionText.text = visualCommand.CommandEntry.Description;
            }

            DynamicScrollContent.verticalScrollbar.value = 1f;
            gameObject.SetActive(true);

            if (!_subscribed) mainWindow.OnWindowSizeChanged += OnMainWindowSizeChanged;
            _subscribed = true;

            InitParams(visualCommand.CommandEntry);
            FavoritesToggle.isOn = visualCommand.CommandEntry.Favorite;
            SetPosition(_parentRect);
            ConsoleEvents.Instance.OpenedParametersWindow();
        }

        void LateUpdate()
        {
            var inp = GetComponentInChildren<InputField>();
            if (IsOpen() && inp != null && inp.isFocused)
            {
                float keyboardHeight = KeyboardTracker.GetSoftKeyboardHeight();

                Vector2 screenKeyboard = new Vector2(0, keyboardHeight * Screen.height);
                Vector2 rectPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(ModalViewTransform, screenKeyboard, null, out rectPoint);

                float holderHeight = Mathf.Max(0, rectPoint.y - ModalViewTransform.rect.min.y);
                if (holderHeight != 0)
                {
                    _keyboardHeightPlace = holderHeight;
                    SetPosition(_parentRect);
                }
            }
            else
            {
                _keyboardHeightPlace = 0;
            }
        }

        public void UpdateDynamicContentHolder()
        {
            float height = 0f;
            for (int i = 0; i < DynamicScrollContent.content.childCount; i++)
            {
                var child = DynamicScrollContent.content.GetChild(i).GetComponent<LayoutElement>();
                height += child.preferredHeight + 5f;
            }
            _dynamicContentHolderLayout.preferredHeight = Mathf.Min(height, 55f);
        }

        public bool IsOpen()
        {
            return gameObject.activeSelf;
        }

        public void Close()
        {
            Clear();
            gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var clickOnModalView = eventData.pointerPressRaycast.gameObject == this.gameObject;
            if (clickOnModalView)
            {
                Close();
            }
        }

        private void OnCloseClick()
        {
            Close();
        }

        private void OnSubmitClick()
        {
            ConsoleCommandsRegistry.InvokeCommand(CurrentCommandEntry, CollectResults());
            Close();
            _currentCommandOnSubmit?.Invoke();
        }

        private void OnFavoritesStateChanged(bool isOn)
        {
            CurrentCommandEntry.SetFavorite(isOn);
        }

        private void OnMainWindowSizeChanged()
        {
            Close();
        }

        private void InitParams(ConsoleCommandEntry commandEntry)
        {
            var parameters = commandEntry.MethodInfo.GetParameters();

            if (parameters == null || parameters.Length == 0)
            {
                DynamicScrollContent.gameObject.SetActive(false);
                SubmitButton.Select();
                return;
            }

            int index = 0;
            foreach (var param in parameters)
            {
                BaseVisualParam visualCommand = VisualParamFactory.CreateVisualParam(this, param.ParameterType);

                if (visualCommand != null)
                {
                    object defaultParam = null;
                    if (commandEntry.LatestParams != null && commandEntry.LatestParams.Length != 0)
                    {
                        defaultParam = commandEntry.LatestParams[index];
                    }
                    visualCommand.Init(param.Name, defaultParam);
                    _visualParams.Add(visualCommand);
                    if (index == 0)
                    {
                        visualCommand.Select();
                    }
                    visualCommand.OnParamSubmitted += () =>
                    {
                        int itemIndex = _visualParams.IndexOf(visualCommand);
                        if (itemIndex < _visualParams.Count - 1)
                        {
                            _visualParams[itemIndex + 1].Select();
                        }
                        else
                        {
                            SubmitButton.Select();
                        }
                    };
                }
                else
                {
                    Jahro.LogError(string.Format(MessagesResource.LogCommandUnsupportedParamters, param.Name, param.ParameterType));
                }
                index++;
            }

            DynamicScrollContent.gameObject.SetActive(true);
            UpdateDynamicContentHolder();
        }

        private object[] CollectResults()
        {
            string debug = "";
            object[] results = new object[_visualParams.Count];
            for (int i = 0; i < _visualParams.Count; i++)
            {
                results[i] = _visualParams[i].GetResult();
                debug += results[i].ToString();
            }
            return results;
        }

        private void Clear()
        {

            foreach (var visualParam in _visualParams)
            {
                visualParam.OnClear();
                GameObject.Destroy(visualParam.gameObject);
            }
            _visualParams.Clear();
        }

        private float CalculateModalViewHeight()
        {
            float spacing = ModalViewTransform.GetComponent<VerticalLayoutGroup>().spacing;
            float size = 0;
            size += TitleText.transform.parent.GetComponent<LayoutElement>().preferredHeight + spacing;
            size += SubmitButton.transform.parent.GetComponent<LayoutElement>().preferredHeight;
            size += DynamicScrollContent.gameObject.activeSelf ? _dynamicContentHolderLayout.preferredHeight + spacing : 0f;
            size += DescriptionText.gameObject.activeSelf ? LayoutUtility.GetPreferredHeight((RectTransform)DescriptionText.transform) + spacing : 0f;
            return size;
        }

        private void SetPosition(RectTransform parentRect)
        {
            var commandRect = CurrentVisualCommand.GetComponent<RectTransform>();

            ModalViewTransform.position = commandRect.position;
            Vector2 deltaX = Vector2.left * commandRect.rect.width / 2f;
            Vector2 deltaY = Vector2.up * commandRect.rect.height / 2f;
            Vector2 pivot = new Vector2(0f, 1f);
            ModalViewTransform.pivot = pivot;
            ModalViewTransform.anchoredPosition += deltaX + deltaY;

            deltaY.y = _keyboardHeightPlace;    //Apply shift from keyboard on Y

            Vector2 parentSize = Vector2.Scale(parentRect.rect.size, parentRect.lossyScale); //real size
            Vector2 modalSize = Vector2.Scale(ModalViewTransform.rect.size, ModalViewTransform.lossyScale); //real size

            if (ModalViewTransform.position.x + modalSize.x > parentSize.x) //if view doesn't fit parent width
            {
                pivot.x = 1f;
                ModalViewTransform.pivot = pivot;
                ModalViewTransform.anchoredPosition += Vector2.right * commandRect.rect.width;
            }

            if (parentSize.x < modalSize.x * 2f)    //if view doesn't fit screen
            {
                pivot.x = 0.5f;
                ModalViewTransform.pivot = pivot;
                float x = parentRect.position.x + parentSize.x / 2f;
                ModalViewTransform.position = new Vector2(x, commandRect.position.y);
                ModalViewTransform.anchoredPosition += Vector2.up * commandRect.rect.height / 2f;
            }

            float modalViewHeight = CalculateModalViewHeight();
            float headerHeight = 52f;
            if (Mathf.Abs(ModalViewTransform.anchoredPosition.y - modalViewHeight + headerHeight) > parentRect.rect.size.y)
            {
                pivot.y = 0f;
                deltaY += Vector2.down * commandRect.rect.height;
            }

            ModalViewTransform.pivot = pivot;
            ModalViewTransform.anchoredPosition += Vector2.up * deltaY;
        }
    }
}