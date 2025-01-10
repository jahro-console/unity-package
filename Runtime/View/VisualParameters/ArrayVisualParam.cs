using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Jahro.View
{
    internal class ArrayVisualParam : BaseVisualParam
    {
        public Text ElementsLength;

        private Slider _slider;

        private string _paramName;

        private ParamsModalView _modalView;

        private System.Type _arrayType;

        private System.Type _elementType;

        private List<BaseVisualParam> _elements = new List<BaseVisualParam>();

        private object[] _defaultParam;

        public override void Init(string paramName, object defaultParam)
        {
            base.Init(paramName, defaultParam);
            _paramName = paramName;

            _slider = GetComponentInChildren<Slider>();
            _slider.onValueChanged.AddListener(OnSliderValueChanged);

            if (defaultParam == null)
            {
                _slider.value = 0;
            }
            else
            {
                IEnumerable array = (IEnumerable)defaultParam;
                int index = 0;
                foreach(var item in array)
                {
                    index++;
                }
                
                _defaultParam = new object[index];
                index = 0;
                foreach(var item in array)
                {
                    _defaultParam[index] = item;
                    index++;
                }
                _slider.value = index;
            }
            
            UpdateParamNameWithSize((int)_slider.value);
        }

        public void SetArray(System.Type type, ParamsModalView modalView)
        {
            _arrayType = type;
            _modalView = modalView;
            if (type == typeof(string[]))
            {
                _elementType = typeof(string);
            }
            else if (type == typeof(int[]))
            {
                _elementType = typeof(int);
            }
            else if (type == typeof(float[]))
            {
                _elementType = typeof(float);
            }
            else if (type == typeof(double[]))
            {
                _elementType = typeof(double);
            }
            else if (type == typeof(bool[]))
            {
                _elementType = typeof(bool);
            }
            else if (type == typeof(Vector3[]))
            {
                _elementType = typeof(Vector3);
            }
            else if (type == typeof(Vector2[]))
            {
                _elementType = typeof(Vector2);
            }
        }

        public override object GetResult()
        {
            int size = _elements.Count;
            
            if (_arrayType == typeof(string[]))
            {
                var array = new string[size];
                for(int i=0; i<size; i++) array[i] = (string)_elements[i].GetResult();
                return array;
            }
            else if (_arrayType == typeof(int[]))
            {
                var intArray = new int[size];
                for(int i=0; i<size; i++) intArray[i] = (int)_elements[i].GetResult();
                return intArray;
            }
            else if (_arrayType == typeof(float[]))
            {
                var array = new float[size];
                for(int i=0; i<size; i++) array[i] = (float)_elements[i].GetResult();
                return array;
            }
            else if (_arrayType == typeof(bool[]))
            {
                var array = new bool[size];
                for(int i=0; i<size; i++) array[i] = (bool)_elements[i].GetResult();
                return array;
            }
            else if (_arrayType == typeof(Vector3[]))
            {
                var array = new Vector3[size];
                for(int i=0; i<size; i++) array[i] = (Vector3)_elements[i].GetResult();
                return array;
            }
            else if (_arrayType == typeof(double[]))
            {
                var array = new double[size];
                for(int i=0; i<size; i++) array[i] = (double)_elements[i].GetResult();
                return array;
            }
            else if (_arrayType == typeof(Vector2[]))
            {
                var array = new Vector2[size];
                for(int i=0; i<size; i++) array[i] = (Vector2)_elements[i].GetResult();
                return array;
            }
            
            return null;
        }

        public override void OnClear()
        {
            base.OnClear();
            foreach(var element in _elements)
            {
                element.OnClear();
                GameObject.Destroy(element.gameObject);
            }
            _elements.Clear();
        }

        private void OnSliderValueChanged(float value)
        {
            int size = Mathf.RoundToInt(value);
            
            UpdateParamNameWithSize(size);
            UpdateElements(size);
        }

        private void UpdateElements(int size)
        {
            if (size > _elements.Count)
            {
                for (int i=_elements.Count; i<size; i++)
                {
                    _elements.Add(CreateElement(i));
                }
            }
            else if (size < _elements.Count) 
            {
                for (int i = _elements.Count-1; i >= size; i--)
                {
                    var element = _elements[i];
                    element.OnClear();
                    GameObject.Destroy(element.gameObject);
                    _elements.RemoveAt(i);
                }
            }

            _modalView.UpdateDynamicContentHolder();
        }

        private BaseVisualParam CreateElement(int elementIndex)
        {   
            var visualElement = VisualParamFactory.CreateVisualParam(_modalView, _elementType);
            object elementDefaultValue = null;
            if (_defaultParam != null && _defaultParam.Length > elementIndex)
            {
                elementDefaultValue = _defaultParam[elementIndex];
            }
            visualElement.Init("element " + elementIndex, elementDefaultValue);
            return visualElement;
        }

        private void UpdateParamNameWithSize(int size)
        {
            ElementsLength.text = ": " + size;
        }
    }
}
