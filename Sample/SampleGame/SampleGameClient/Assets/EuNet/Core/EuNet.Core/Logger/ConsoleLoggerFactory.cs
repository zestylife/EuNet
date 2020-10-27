namespace EuNet.Core
{
    public class ConsoleLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string name)
        {
            return new ConsoleLogger(name);
        }
    }
}
