using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jahro.Core.Commands
{
    [Serializable]
    internal class CommandsQueue
    {
        const int LIMIT = 20;

        [SerializeField]
        private List<string> _commands;

        private int _currentIndex;

        public int CurrentIndex { get { return _currentIndex; }}

        internal CommandsQueue()
        {
            _commands = new List<string>();
        }

        public void PushCommand(string value)
        {
            if (_commands.Count > 0)
            {
                string prev = _commands[_commands.Count-1];
                if (value == prev)
                {
                    _currentIndex = _commands.Count;
                    return;
                }
            }
            _commands.Add(value);
            if (_commands.Count > LIMIT)
            {
                _commands.RemoveAt(0);
            }
            _currentIndex = _commands.Count;
        }

        public string GetUp()
        {
            if (_commands.Count == 0)
            {
                return "";
            }

            _currentIndex--;
            if (_currentIndex < 0)
            {
                _currentIndex = 0;
            }
            return _commands[_currentIndex];
        }

        public string GetDown()
        {
            if (_commands.Count == 0)
            {
                return "";
            }
            
            _currentIndex++;
            if (_currentIndex > _commands.Count-1)
            {
                _currentIndex = _commands.Count-1;
            }
            return _commands[_currentIndex];
        }

        public void ResetIndex()
        {
            _currentIndex = _commands.Count;
        }
    }
}