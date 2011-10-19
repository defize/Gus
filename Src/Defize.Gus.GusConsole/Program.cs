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

            Console.WriteLine("Defize Gus SQL Script Runner {0}", version.ToString(3));
            Console.WriteLine();

            Parser.Run<ConsoleApp>(args);
        }
    }
}
