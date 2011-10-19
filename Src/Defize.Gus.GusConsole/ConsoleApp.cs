namespace Defize.Gus.GusConsole
{
    using System;
    using CLAP;
    using CLAP.Validation;

    internal class ConsoleApp
    {
        [Verb(Aliases = "a", Description = "Apply the specifed SQL scripts to the specified database.")]
        public static void Apply(
            [Parameter(Aliases = "src", Description = "The source folder.")]
            [DirectoryExists]
            string source,
            [Parameter(Aliases = "svr", Description = "The destination server.", Required = true)]
            string server,
            [Parameter(Aliases = "db", Description = "The name of the database.", Required = true)]
            string database)
        {
            var configuration = new ApplyTaskConfiguration
                                    {
                                        Server = server,
                                        Database = database
                                    };

            var task = new ApplyTask();
            task.ExecutionEvent += TaskExecutionEventHandler;
            try
            {
                task.Execute(configuration);
            }
            finally
            {
                task.ExecutionEvent -= TaskExecutionEventHandler;
            }
        }

        [Verb(Aliases = "c", Description = "Create a new SQL script with a unique timestamped name.")]
        public static void Create()
        {

        }

        [Verb(Aliases = "v", Description = "Verify the specified SQL scripts against the specified database.")]
        public static void Verify()
        {

        }

        [Empty]
        [Help(Aliases = "h,?")]
        public static void Help(string help)
        {
            Console.WriteLine(help);
        }

        [Error]
        public static void Error(Exception ex)
        {
            Console.WriteLine("The horrors.");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        private static void TaskExecutionEventHandler(object sender, GusTaskExecutionEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
