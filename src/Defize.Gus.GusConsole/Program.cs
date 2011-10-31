namespace Defize.Gus.GusConsole
{
    using System.Reflection;
    using CLAP;
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;

            ConsoleHelper.WriteRule();
            ConsoleHelper.WriteCentered(string.Format("Defize Gus SQL Script Runner {0}", version.ToString(3)));
            ConsoleHelper.WriteRule();
            Console.WriteLine();

            Parser.Run<ConsoleApp>(args);
        }
    }
}
