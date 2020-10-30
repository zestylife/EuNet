using System;

namespace EuNet.Core
{
    public class DefaultLoggerFactory : ILoggerFactory
    {
        private LoggerFactoryBuilder _builder;

        private DefaultLoggerFactory()
        {
            _builder = new LoggerFactoryBuilder();
        }

        public static DefaultLoggerFactory Create(Action<LoggerFactoryBuilder> creator)
        {
            var factory = new DefaultLoggerFactory();
            creator(factory._builder);
            return factory;
        }

        public ILogger CreateLogger(string name)
        {
            return new DefaultLogger(name, _builder);
        }

        public ILogger CreateLogger<T>()
        {
            return CreateLogger(nameof(T));
        }
    }
}
