using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Jahro.View
{
    internal abstract class BaseVisualParam : MonoBehaviour
    {
        public Text ParamNameText;

        public Action OnParamSubmitted;

        public virtual void Init(string paramName, object defaultValue)
        {
            SetParamName(paramName);
        }

        public abstract object GetResult();

        protected void SetParamName(string name)
        {
            ParamNameText.text = name;
        }

        public virtual void OnClear()
        {

        }

        public void Select()
        {
            OnSelect();
        }

        protected virtual void OnSelect()
        {

        }
    }
}