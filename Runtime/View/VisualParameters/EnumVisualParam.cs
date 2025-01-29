using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace JahroConsole.View
{
    internal class EnumVisualParam : BaseVisualParam
    {

        public Dropdown Dropdown;

        private string[] _enumNames;

        private Type _enumType;

        private object _defaultParam;

        public override void Init(string paramName, object defaultParam)
        {
            base.Init(paramName, defaultParam);
            _defaultParam = defaultParam;

            if (_defaultParam != null)
            {
                int index = Array.IndexOf(_enumNames, _defaultParam.ToString());
                Dropdown.value = index;
            }
        }

        public void SetEnumOptions(Type type)
        {
            _enumType = type;
            _enumNames = Enum.GetNames(type);
            List<string> names = new List<string>();
            names.AddRange(_enumNames);

            Dropdown.AddOptions(names);
        }

        public override object GetResult()
        {
            var selectedName = _enumNames[Dropdown.value];
            return Enum.Parse(_enumType, selectedName);
        }
    }
}