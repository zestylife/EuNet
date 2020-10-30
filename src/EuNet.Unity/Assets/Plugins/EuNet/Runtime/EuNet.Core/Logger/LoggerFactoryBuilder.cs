using System;
using System.Collections.Generic;

namespace EuNet.Core
{
    public class LoggerFactoryBuilder : ILogger
    {
        public LogLevel MinimumLogLevel { get; private set; } = LogLevel.Error;
        private List<ILogger> _loggers = new List<ILogger>();

        public void SetMinimumLevel(LogLevel logLevel)
        {
            MinimumLogLevel = logLevel;
        }

        public void AddLogger(ILogger logger)
        {
            _loggers.Add(logger);
        }

        public void AddConsoleLogger()
        {
            AddLogger(new ConsoleLogger());
        }

        public void LogError(Exception e, string msg)
        {
            foreach (var logger in _loggers)
                logger.LogError(e, msg);
        }

        public void LogError(string msg)
        {
            foreach (var logger in _loggers)
                logger.LogError(msg);
        }

        public void LogInformation(string msg)
        {
            foreach (var logger in _loggers)
                logger.LogInformation(msg);
        }

        public void LogWarning(string msg)
        {
            foreach (var logger in _loggers)
                logger.LogWarning(msg);
        }
    }
}
