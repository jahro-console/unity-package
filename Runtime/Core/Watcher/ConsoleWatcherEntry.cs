using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JahroConsole.Core.Registry;

namespace JahroConsole.Core.Watcher
{
    internal class ConsoleWatcherEntry : ConsoleEntry
    {
        internal MemberInfo MemberInfo { get; private set; }

        internal bool IsRuntime { get; private set; }

        internal object RuntimeObject { get; private set; }

        protected StringBuilder _stringBuilder;

        private object _lastValueObject;

        internal ConsoleWatcherEntry(string name, string description) : base(name, description)
        {
            _stringBuilder = new StringBuilder();
        }

        internal void SetMemberInfo(MemberInfo memberInfo)
        {
            MemberInfo = memberInfo;
        }

        internal void SetRuntimeObject(object obj)
        {
            RuntimeObject = obj;
            IsRuntime = true;
        }

        internal virtual string GetTypeDescription()
        {
            string description = "";
            description += IsRuntime ? "(Runtime " : "(";
            description += MemberInfo.MemberType.ToString() + " of " + GetMemberType().Name + ")";
            return description;
        }

        internal virtual Type GetMemberType()
        {
            Type type = null;
            var memberInfo = MemberInfo;
            if (memberInfo.MemberType == MemberTypes.Field)
            {
                type = ((FieldInfo)memberInfo).FieldType;
            }
            else if (memberInfo.MemberType == MemberTypes.Property)
            {
                type = ((PropertyInfo)memberInfo).PropertyType;
            }
            return type;
        }

        internal virtual string GetShortStringValue(out int size)
        {
            try
            {
                WatcherStringMapper.MapValueToShortString(this, GetValue(), _stringBuilder, out size);
            }
            catch (Exception e)
            {
                WatcherStringMapper.MapValueToShortString(this, e, _stringBuilder, out size);
            }
            return _stringBuilder.ToString();
        }

        internal virtual string GetFullDetails()
        {
            var newValue = GetValue();
            if (_lastValueObject != newValue || GetMemberType().IsPrimitive == false)
            {
                WatcherStringMapper.MapValueToDetailsString(this, newValue, _stringBuilder);
                _lastValueObject = newValue;
            }
            return _stringBuilder.ToString();
        }

        private object GetValue()
        {
            object currentValue = null;
            if (MemberInfo.MemberType == MemberTypes.Field)
            {
                try
                {
                    FieldInfo fieldInfo = MemberInfo as FieldInfo;
                    if (IsRuntime)
                    {
                        currentValue = fieldInfo.GetValue(RuntimeObject);
                    }
                    else
                    {
                        currentValue = fieldInfo.GetValue(null);
                    }
                }
                catch (Exception e)
                {
                    currentValue = e;
                }
            }
            else if (MemberInfo.MemberType == MemberTypes.Property)
            {
                PropertyInfo propertyInfo = MemberInfo as PropertyInfo;
                try
                {
                    if (IsRuntime)
                    {
                        currentValue = propertyInfo.GetValue(RuntimeObject);
                    }
                    else
                    {
                        currentValue = propertyInfo.GetValue(null);
                    }
                }
                catch (Exception e)
                {
                    currentValue = e;
                }
            }
            return currentValue;
        }

        public override bool Equals(object obj)
        {
            return obj is ConsoleWatcherEntry entry &&
                   base.Equals(obj) &&
                   EqualityComparer<MemberInfo>.Default.Equals(MemberInfo, entry.MemberInfo) &&
                   EqualityComparer<object>.Default.Equals(RuntimeObject, entry.RuntimeObject);
        }

        public override int GetHashCode()
        {
            return new { Name, Description, MemberInfo, RuntimeObject }.GetHashCode();
        }
    }
}