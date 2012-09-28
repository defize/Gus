namespace Defize.Gus
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;

    public class StatusTask
    {
        public bool Execute(StatusTaskConfiguration configuration, GusTaskExecutionContext context)
        {
            var databaseHelper = new DatabaseHelper(context);
            var fileSystemHelper = new FileSystemHelper(context);

            var connection = databaseHelper.CreateAndOpenConnection(configuration.Server);
            if (connection == null)
            {
                return false;
            }

            var serverConnection = new ServerConnection(connection);
            var server = new Server(serverConnection);

            var database = databaseHelper.InitializeDatabase(server, configuration.Database, false, false);
            if (database == null)
            {
                return false;
            }

            context.RaiseExecutionEvent("Determining scripts not yet applied.");
            var scriptsToApply = GetScriptsToApply(configuration.SourcePath, database, databaseHelper, fileSystemHelper);
            if (scriptsToApply == null)
            {
                return false;
            }

            context.ExecutionStepCount = (uint)scriptsToApply.Count;
            context.RaiseExecutionEvent(string.Format("Found {0} scripts not yet applied.", scriptsToApply.Count));

            foreach (var script in scriptsToApply)
            {
                context.RaiseExecutionEvent(ExecutionEventType.Default, script.Name);
            }

            return true;
        }

        private static ICollection<FileInfo> GetScriptsToApply(string sourcePath, Database database, DatabaseHelper databaseHelper, FileSystemHelper fileSystemHelper)
        {
            var scriptFiles = fileSystemHelper.GetScriptFiles(sourcePath);
            if (scriptFiles == null)
            {
                return null;
            }

            var previouslyAppliedScripts = databaseHelper.GetPreviouslyAppliedScripts(database);
            var appliedScriptsLookup = previouslyAppliedScripts.ToDictionary(x => x.Filename, x => x.Hash);

            return scriptFiles.Where(x => !appliedScriptsLookup.ContainsKey(x.Name)).OrderBy(x => x.Name).ToList();
        }
    }
}
