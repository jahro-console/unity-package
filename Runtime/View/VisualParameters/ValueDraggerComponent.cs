using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace JahroConsole.View
{
    internal class ValueDraggerComponent : MonoBehaviour
    {
        public InputField InputField;

        public Selectable Slider;

        public Action<float> OnDragValueChanged = delegate { };

        public Action OnDragValueApply = delegate { };

        public Action<string> OnValueSubmitted = delegate { };

        public string Value
        {
            get
            {
                return InputField.text;
            }
            set
            {
                InputField.text = value;
            }
        }

        void Awake()
        {
            var _sliderEventTrigger = Slider.gameObject.AddComponent<EventTrigger>();
            var pointerUp = new EventTrigger.Entry();
            pointerUp.eventID = EventTriggerType.PointerUp;
            pointerUp.callback.AddListener(OnPointerUp);
            _sliderEventTrigger.triggers.Add(pointerUp);

            var pointerDown = new EventTrigger.Entry();
            pointerDown.eventID = EventTriggerType.PointerDown;
            pointerDown.callback.AddListener(OnPointerDown);
            _sliderEventTrigger.triggers.Add(pointerDown);

            var pointerDrag = new EventTrigger.Entry();
            pointerDrag.eventID = EventTriggerType.Drag;
            pointerDrag.callback.AddListener(OnPointerDrag);
            _sliderEventTrigger.triggers.Add(pointerDrag);

            InputField.onSubmit.AddListener(OnSubmit);
        }

        private void OnSubmit(string arg0)
        {
            OnValueSubmitted(arg0);
        }

        public void Select()
        {
            InputField.Select();
        }

        private void OnPointerDrag(BaseEventData arg0)
        {
            var pointerData = (PointerEventData)arg0;
            OnDragValueChanged(pointerData.position.x - pointerData.pressPosition.x);
        }

        private void OnPointerDown(BaseEventData arg0)
        {
            OnDragValueApply();
        }

        private void OnPointerUp(BaseEventData arg0)
        {
            OnDragValueApply();
        }
    }
}