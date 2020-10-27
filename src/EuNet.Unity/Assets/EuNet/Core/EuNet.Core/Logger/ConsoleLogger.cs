using System;

namespace EuNet.Core
{
    public class ConsoleLogger : ILogger
    {
        private readonly string _name;

        public ConsoleLogger(string name)
        {
            _name = name;
        }

        public void LogError(Exception e, string msg)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{_name}] {msg}\n{e.Message}\n{e.StackTrace}");
            Console.ForegroundColor = color;
        }

        public void LogError(string msg)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{_name}] {msg}");
            Console.ForegroundColor = color;
        }

        public void LogInformation(string msg)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"[{_name}] {msg}");
            Console.ForegroundColor = color;
        }

        public void LogWarning(string msg)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{_name}] {msg}");
            Console.ForegroundColor = color;
        }
    }
}

