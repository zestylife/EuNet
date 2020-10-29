using System;
using UnityEngine;

namespace EuNet.Unity
{
    public class UnityDebugLogger : Core.ILogger
    {
        private readonly string _name;

        public UnityDebugLogger(string name)
        {
            _name = name;
        }

        public void LogError(Exception e, string msg)
        {
            Debug.LogError($"[{_name}] {msg}\n{e.Message}\n{e.StackTrace}");
        }

        public void LogError(string msg)
        {
            Debug.LogError($"[{_name}] {msg}");
        }

        public void LogInformation(string msg)
        {
            Debug.Log($"[{_name}] {msg}");
        }

        public void LogWarning(string msg)
        {
            Debug.LogWarning($"[{_name}] {msg}");
        }
    }
}

