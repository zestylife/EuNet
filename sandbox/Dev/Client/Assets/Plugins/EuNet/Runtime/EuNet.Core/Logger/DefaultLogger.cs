using System;

namespace EuNet.Core
{
    public class DefaultLogger : ILogger
    {
        private readonly string _name;
        private readonly LoggerFactoryBuilder _builder;

        public DefaultLogger(string name, LoggerFactoryBuilder builder)
        {
            _name = name;
            _builder = builder;
        }

        public void LogError(Exception e, string msg)
        {
            if (_builder.MinimumLogLevel < LogLevel.Error)
                return;

            _builder.LogError(e, $"[{_name}] {msg}\n{e.Message}\n{e.StackTrace}");
        }

        public void LogError(string msg)
        {
            if (_builder.MinimumLogLevel < LogLevel.Error)
                return;

            _builder.LogError($"[{_name}] {msg}");
        }

        public void LogWarning(string msg)
        {
            if (_builder.MinimumLogLevel < LogLevel.Warning)
                return;

            _builder.LogWarning($"[{_name}] {msg}");
        }

        public void LogInformation(string msg)
        {
            if (_builder.MinimumLogLevel < LogLevel.Information)
                return;

            _builder.LogInformation($"[{_name}] {msg}");
        }
    }
}

