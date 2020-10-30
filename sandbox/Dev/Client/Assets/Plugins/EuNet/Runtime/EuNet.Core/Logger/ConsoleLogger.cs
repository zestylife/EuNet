using System;

namespace EuNet.Core
{
    public class ConsoleLogger : ILogger
    {
        public ConsoleLogger()
        {
            
        }

        public void LogError(Exception e, string msg)
        {
            Console.Error.WriteLine(msg);
        }

        public void LogError(string msg)
        {
            Console.Error.WriteLine(msg);
        }

        public void LogWarning(string msg)
        {
            Console.Error.WriteLine(msg);
        }

        public void LogInformation(string msg)
        {
            Console.Out.WriteLine(msg);
        }
    }
}

