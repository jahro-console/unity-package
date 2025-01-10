using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Jahro.View
{
    internal static class VisualParamFactory
    {
        public static BaseVisualParam CreateVisualParam(ParamsModalView paramsView, System.Type type)
        {
            var contentHolder = paramsView.ParamsContentHolder;
            BaseVisualParam visualParam = null;
            if (type == typeof(string))
            {
                visualParam = GameObject.Instantiate(paramsView.StringParamPrefab).GetComponent<BaseVisualParam>();
            }
            else if (type == typeof(int))
            {
                visualParam = GameObject.Instantiate(paramsView.IntParamPrefab).GetComponent<BaseVisualParam>();
            }
            else if (type == typeof(bool))
            {
                visualParam = GameObject.Instantiate(paramsView.BoolParamPrefab).GetComponent<BaseVisualParam>();
            }
            else if (type == typeof(float))
            {
                visualParam = GameObject.Instantiate(paramsView.FloatParamPrefab).GetComponent<BaseVisualParam>();
            }
            else if (type == typeof(double))
            {
                visualParam = GameObject.Instantiate(paramsView.DoubleParamPrefab).GetComponent<BaseVisualParam>();
            }
            else if (type == typeof(Vector2))
            {
                visualParam = GameObject.Instantiate(paramsView.Vector2ParamPrefab).GetComponent<BaseVisualParam>();
            }
            else if (type == typeof(Vector3))
            {
                visualParam = GameObject.Instantiate(paramsView.Vector3ParamPrefab).GetComponent<BaseVisualParam>();
            }
            else if (type.IsEnum)
            {
                var enumParam = GameObject.Instantiate(paramsView.EnumParamPrefab).GetComponent<EnumVisualParam>();
                enumParam.SetEnumOptions(type);
                visualParam = enumParam;
            }
            else if (type.IsArray)
            {
                var arrayParam = GameObject.Instantiate(paramsView.ArrayParamPrefab).GetComponent<ArrayVisualParam>();
                arrayParam.SetArray(type, paramsView);
                visualParam = arrayParam;
            }

            if (visualParam != null)
            {
                var rectTransform = visualParam.gameObject.GetComponent<RectTransform>();
                rectTransform.SetParent(contentHolder);
                rectTransform.localScale = Vector3.one;
            }

            return visualParam;
        }
    }
}