using System;

namespace EuNet.Core
{
    public interface ILogger
    {
        void LogError(Exception e, string msg);
        void LogError(string msg);
        void LogWarning(string msg);
        void LogInformation(string msg);
    }
}

