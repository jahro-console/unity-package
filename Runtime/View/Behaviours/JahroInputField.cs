using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Jahro.View
{
    [RequireComponent(typeof(InputField))]
    internal class JahroInputField : MonoBehaviour
    {
        public Action<string> onSubmit;

        private InputField inputField;

        void Awake()
        {
            inputField = GetComponent<InputField>();
            inputField.lineType = InputField.LineType.MultiLineNewline;
        }

        void OnEnable()
        {
            inputField.onValidateInput += CheckForEnter;
        }

        void OnDisable()
        {
            inputField.onValidateInput -= CheckForEnter;
        }

        private char CheckForEnter(string text, int charIndex, char addedChar)
        {
            if (addedChar == '\n' && onSubmit != null)
            {
                onSubmit.Invoke(text);
                return '\0';
            }
            else
                return addedChar;
        }
    }
}