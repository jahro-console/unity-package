using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JahroConsole.Core.Registry;

namespace JahroConsole.Core.Commands
{
    internal class ConsoleCommandEntry : ConsoleEntry
    {
        internal MethodInfo MethodInfo { get; private set; }

        internal Delegate DelegateInfo { get; private set; }

        internal object ReferenceObject { get; private set; }

        internal bool DelegateType { get; private set; }

        internal bool Runtime { get; private set; }

        internal string SimpleName { get; private set; }

        internal object[] LatestParams { get; set; }

        internal DateTime LastExecuted { get; set; }

        internal ConsoleCommandsInvoker.ICommandInvoker Invoker { get; set; }

        internal Action<object> OnExecuted;

        internal ConsoleCommandEntry(string name, string description) : base(name, description)
        {

        }

        internal void SetMethodInfo(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            SimpleName = GetSimpleName();
            LatestParams = GetParametersDefaultValues();
            DelegateType = false;
        }

        internal void SetDelegateInfo(Delegate delegateInfo)
        {
            MethodInfo = delegateInfo.GetMethodInfo();
            SimpleName = GetSimpleName();
            LatestParams = GetParametersDefaultValues();
            DelegateInfo = delegateInfo;
            DelegateType = true;
        }

        internal void SetReferenceObject(object obj)
        {
            ReferenceObject = obj;
        }

        internal void SetRuntime(bool runtime)
        {
            Runtime = runtime;
        }

        internal void SetInvoker(ConsoleCommandsInvoker.ICommandInvoker invoker)
        {
            Invoker = invoker;
        }

        internal void Executed(object[] parameters, object result)
        {
            LatestParams = parameters;
            LastExecuted = DateTime.Now;
            OnExecuted(result);
        }

        internal string GetSimpleName()
        {
            string result = Name;
            var parameters = MethodInfo.GetParameters();
            foreach (var p in parameters)
            {
                result += "|" + p.ParameterType.Name;
            }
            return result;
        }

        internal string GetReadableParameters()
        {
            StringBuilder sb = new StringBuilder();

            var parameters = MethodInfo.GetParameters();
            if (parameters.Length > 0)
            {
                sb.Append("(");
            }
            else
            {
                return "()";
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                sb.Append(parameters[i].ParameterType.Name.ToLower());
                if (i + 1 < parameters.Length)
                {
                    sb.Append(",");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        private object[] GetParametersDefaultValues()
        {
            var parametersInfo = MethodInfo.GetParameters();
            if (parametersInfo.Length == 0)
            {
                return null;
            }

            object[] defaultValues = new object[parametersInfo.Length];
            for (int i = 0; i < parametersInfo.Length; i++)
            {
                var parameter = parametersInfo[i];
                if (parameter.HasDefaultValue)
                {
                    defaultValues[i] = parameter.DefaultValue;
                }
            }
            return defaultValues;
        }

        public override bool Equals(object obj)
        {
            return obj is ConsoleCommandEntry entry &&
                   base.Equals(obj) &&
                   EqualityComparer<MethodInfo>.Default.Equals(MethodInfo, entry.MethodInfo) &&
                   SimpleName == entry.SimpleName;
        }

        public override int GetHashCode()
        {
            return new { Name, Description, MethodInfo, SimpleName }.GetHashCode();
        }
    }
}