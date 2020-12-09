using EuNet.Core;
using System;
using UnityEngine;

namespace EuNet.Unity
{
    public class UnityDebugLogger : Core.ILogger
    {
        public UnityDebugLogger()
        {
            
        }

        public void LogError(Exception e, string msg)
        {
            Debug.LogException(e);
        }

        public void LogError(string msg)
        {
            Debug.LogError(msg);
        }

        public void LogInformation(string msg)
        {
            Debug.Log(msg);
        }

        public void LogWarning(string msg)
        {
            Debug.LogWarning(msg);
        }
    }

    public static class UnityDebugLoggerExtentions
    {
        public static void AddUnityDebugLogger(this LoggerFactoryBuilder builder)
        {
            builder.AddLogger(new UnityDebugLogger());
        }
    }
}

