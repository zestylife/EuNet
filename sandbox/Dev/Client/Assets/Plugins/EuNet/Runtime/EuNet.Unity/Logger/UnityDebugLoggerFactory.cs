using EuNet.Core;

namespace EuNet.Unity
{
    public class UnityDebugLoggerFactory : ILoggerFactory
    {
        public ILogger CreateLogger(string name)
        {
            return new UnityDebugLogger(name);
        }
    }
}
