namespace Defize.Gus
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;
    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;

    public class ApplyTask
    {
        public bool Execute(ApplyTaskConfiguration configuration, GusTaskExecutionContext context)
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

            var database = databaseHelper.InitializeDatabase(server, configuration.Database, configuration.CreateDatabaseIfMissing, configuration.CreateDatabaseIfMissing || configuration.CreateManagementSchemaIfMissing);
            if (database == null)
            {
                return false;
            }

            context.RaiseExecutionEvent("Determining scripts to apply.");
            var scriptsToApply = GetScriptsToApply(configuration.SourcePath, database, databaseHelper, fileSystemHelper);
            if (scriptsToApply == null)
            {
                return false;
            }

            context.ExecutionStepCount = (uint)scriptsToApply.Count;
            context.RaiseExecutionEvent(string.Format("Found {0} scripts to apply.", scriptsToApply.Count));

            var success = ApplyScripts(scriptsToApply, server, database, databaseHelper, fileSystemHelper, configuration.RecordOnly, context, configuration.HaltOnError);

            return success;
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

        private static bool ApplyScripts(IEnumerable<FileInfo> scriptsToApply, Server server, Database database, DatabaseHelper databaseHelper, FileSystemHelper fileSystemHelper, bool recordOnly, GusTaskExecutionContext context, bool haltOnError)
        {
            uint index = 0;
            var hasErrors = false;

            foreach (var script in scriptsToApply)
            {
                ++index;

                server.ConnectionContext.BeginTransaction();
                try
                {
                    var hash = fileSystemHelper.GetFileHash(script);

                    if (!recordOnly)
                    {
                        using (var sr = script.OpenText())
                        {
                            database.ExecuteNonQuery(sr.ReadToEnd());
                        }
                    }

                    databaseHelper.RecordScript(database, script.Name, hash);

                    server.ConnectionContext.CommitTransaction();

                    context.RaiseExecutionEvent(ExecutionEventType.Default, string.Format("Applied script '{0}'.", script.Name), index);
                }
                catch (FailedOperationException ex)
                {
                    server.ConnectionContext.RollBackTransaction();

                    Exception sqlException = FindSqlException(ex);
                    var exceptionMessage = (sqlException ?? ex).Message;

                    context.RaiseExecutionEvent(ExecutionEventType.Error, string.Format("Error applying script '{0}': {1}", script.Name, exceptionMessage), index);

                    if (haltOnError)
                    {
                        return false;
                    }

                    hasErrors = true;
                }
            }

            return !hasErrors;
        }

        private static SqlException FindSqlException(Exception ex)
        {
            while (ex.InnerException != null)
            {
                var sqlException = ex.InnerException as SqlException;
                if (sqlException != null)
                {
                    return sqlException;
                }

                ex = ex.InnerException;
            }

            return null;
        }
    }
}
