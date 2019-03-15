using System;

namespace IHostedServiceAsAService
{
    internal class ConsoleLogger : ILogger
    {
        public void Info(string str)
        {
            WriteLine($"{DateTime.Now:G} -> {str}");
        }

        public void InportantInfo(string str)
        {
            WriteLine(str, ConsoleColor.Green);
        }

        public void Warning(string str)
        {
            WriteLine($"Warning: {str}", ConsoleColor.Yellow);
        }

        public void Error(string str)
        {
            WriteLine($"ERROR: {str}", ConsoleColor.Red);
        }

        private static void WriteLine(string str, ConsoleColor? color = null)
        {
            ConsoleColor currentColor = default;
            if (color.HasValue)
            {
                currentColor = Console.ForegroundColor;
                Console.ForegroundColor = color.Value;
            }

            Console.WriteLine(str);

            if (color.HasValue)
                Console.ForegroundColor = currentColor;
        }
    }
}