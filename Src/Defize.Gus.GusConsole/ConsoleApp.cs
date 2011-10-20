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
            string database,
            [Parameter(Aliases = "cd", Description = "Creates the database if missing.", Default = false)]
            bool createDatabaseIfMissing,

            [Parameter(Aliases = "cms", Description = "Creates the Gus schema if missing.", Default = true)]
            bool createManagementSchemaIfMissing,
            [Parameter(Aliases = "ro", Description = "Register scripts without executing.", Default = false)]
            bool recordOnly,
            [Parameter(Aliases = "hoe", Description = "Stop processing scripts when there is an error", Default = true)]
            bool haltOnError)
        {
            var configuration = new ApplyTaskConfiguration
                                    {
                                        Server = server,
                                        Database = database,
                                        CreateDatabaseIfMissing = createDatabaseIfMissing,
                                        CreateManagementSchemaIfMissing = createManagementSchemaIfMissing,
                                        RecordOnly = recordOnly,
                                        SourcePath = source,
                                        HaltOnError = haltOnError
                                    };

            var context = new GusTaskExecutionContext();
            context.ExecutionEvent += TaskExecutionEventHandler;
            var task = new ApplyTask();

            try
            {
                var success = task.Execute(configuration, context);

                if (success)
                {
                    DisplayExecutionEvent(new GusTaskExecutionEventArgs(ExecutionEventType.Success, "Task completed successfully.", 0));
                }
                else
                {
                    DisplayExecutionEvent(new GusTaskExecutionEventArgs(ExecutionEventType.Error, "Task completed with errors.", 0));
                }
            }
            finally
            {
                context.ExecutionEvent -= TaskExecutionEventHandler;
            }
        }

        [Verb(Aliases = "c", Description = "Create a new SQL script with a unique timestamped name.")]
        public static void Create(
            [Parameter(Aliases = "n", Description = "The name of the script to create.", Required = true)]
            string name)
        {
            var configuration = new CreateTaskConfiguration
            {
                Name = name
            };

            var context = new GusTaskExecutionContext();
            context.ExecutionEvent += TaskExecutionEventHandler;
            var task = new CreateTask();

            try
            {
                var success = task.Execute(configuration, context);

                if (success)
                {
                    DisplayExecutionEvent(new GusTaskExecutionEventArgs(ExecutionEventType.Success, "Task completed successfully.", 0));
                }
                else
                {
                    DisplayExecutionEvent(new GusTaskExecutionEventArgs(ExecutionEventType.Error, "Task completed with errors.", 0));
                }
            }
            finally
            {
                context.ExecutionEvent -= TaskExecutionEventHandler;
            }
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
            DisplayExecutionEvent(e);
        }

        private static void DisplayExecutionEvent(GusTaskExecutionEventArgs e)
        {
            ConsoleHelper.WriteExecutionEventToConsole(e);
        }
    }
}
