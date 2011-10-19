namespace Defize.Gus
{
    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;

    public class ApplyTask
    {
        public bool Execute(ApplyTaskConfiguration configuration, GusTaskExecutionContext context)
        {
            var databaseHelper = new DatabaseHelper(context);
            
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

            return true;
        }
    }
}
