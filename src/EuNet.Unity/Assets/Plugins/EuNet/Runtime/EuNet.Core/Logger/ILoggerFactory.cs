namespace EuNet.Core
{
    public interface ILoggerFactory
    {
        ILogger CreateLogger(string name);
        ILogger CreateLogger<T>();
    }
}