namespace Defize.Gus.GusConsole
{
    using System;

    internal class ConsoleHelper
    {
        public static void WriteExecutionEventToConsole(GusTaskExecutionEventArgs args)
        {
            switch (args.Type)
            {
                case ExecutionEventType.Success:
                    WriteColor("[SUCCESS] ", ConsoleColor.Green);
                    break;

                case ExecutionEventType.Warning:
                    WriteColor("[WARNING] ", ConsoleColor.Yellow);
                    break;

                case ExecutionEventType.Error:
                    WriteColor("[ERROR] ", ConsoleColor.Red);
                    break;

                case ExecutionEventType.Failure:
                    WriteColor("[FAILURE] ", ConsoleColor.Red);
                    break;
            }

            Console.WriteLine(args.Message);
        }

        public static void WriteColor(string value, ConsoleColor color)
        {
            var currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ForegroundColor = currentColor;
        }

        public static void WriteRule()
        {
            var rule = new String('-', Console.WindowWidth - 1);
            Console.WriteLine(rule);
        }

        public static void WriteCentered(string value)
        {
            var padding = ((Console.WindowWidth - 1) - value.Length) / 2;
            Console.Write(new string(' ', padding));
            Console.WriteLine(value);
        }
    }
}
