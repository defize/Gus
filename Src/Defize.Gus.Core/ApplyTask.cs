namespace Defize.Gus
{
    using System;
    using System.Data.SqlClient;
    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;

    public class ApplyTask : GusTaskBase
    {
        private const string SchemaName = "Gus";
        private const string HistoryTableName = "GusHistory";
        private const string HistoryTablePrimaryKeyName = "PK_Gus_GusHistory";
        private const string HistoryTableFilenameColumnName = "Filename";
        private const string HistoryTableHashColumnName = "Hash";
        private const string HistoryTableAppliedOnColumnName = "AppliedOn";
        private const string HistoryTableAppliedByColumnName = "AppliedBy";
        private const string RegisterSqlScript = "INSERT INTO [Gus].[GusHistory] ([Filename], [Hash], [AppliedOn], [AppliedBy]) VALUES (N'{0}', N'{1}', GETDATE(), SYSTEM_USER)";

        public bool Execute(ApplyTaskConfiguration configuration)
        {
            OnExecutionEvent(string.Format("Connecting to database '{0}'.", configuration.Server));
            var connection = CreateAndOpenConnection(configuration);
            if (connection == null)
            {
                return false;
            }

            var serverConnection = new ServerConnection(connection);
            var server = new Server(serverConnection);

            OnExecutionEvent("Initialising database.");
            var database = InitializeDatabase(server, configuration.Database, false, false);

            return false;
        }

        private SqlConnection CreateAndOpenConnection(ApplyTaskConfiguration configuration)
        {
            var connectionBuilder = new SqlConnectionStringBuilder {DataSource = configuration.Server, IntegratedSecurity = true};
            var connectionString = connectionBuilder.ToString();

            var connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
            }
            catch (SqlException ex)
            {
                OnExecutionEvent(ExecutionEventType.Error, string.Format("Unable to open connection to database: {0}", ex.Message));
                return null;
            }

            return connection;
        }

        private Database InitializeDatabase(Server server, string databaseName, bool createDatabaseIfMissing, bool createManagementSchemaIfMissing)
        {
            var database = OpenDatabase(server, databaseName, createDatabaseIfMissing);
            if (database == null)
            {
                return null;
            }

            var isSchemaValid = CheckSchema(database, createManagementSchemaIfMissing);
            if (!isSchemaValid)
            {
                return null;
            }

            var isHistoryTableValid = CheckHistoryTable(database, createManagementSchemaIfMissing);
            if (!isHistoryTableValid)
            {
                return null;
            }

            return database;
        }

        private Database OpenDatabase(Server server, string databaseName, bool createDatabaseIfMissing)
        {
            var database = server.Databases[databaseName];
            if (database == null)
            {
                if (createDatabaseIfMissing)
                {
                    database = new Database(server, databaseName);
                    database.Create();
                }
                else
                {
                    OnExecutionEvent(ExecutionEventType.Error, string.Format("The database '{0}' could not be found.", databaseName));
                    return null;
                }
            }

            return database;
        }

        private bool CheckSchema(Database database, bool createManagementSchemaIfMissing)
        {
            var schema = database.Schemas[SchemaName];
            if (schema == null)
            {
                if (createManagementSchemaIfMissing)
                {
                    schema = new Schema(database, SchemaName);
                    schema.Create();
                }
                else
                {
                    OnExecutionEvent(ExecutionEventType.Error, "The Gus management schema could not be found.");
                    return false;
                }
            }

            return true;
        }

        private bool CheckHistoryTable(Database database, bool createManagementSchemaIfMissing)
        {
            var historyTable = database.Tables[HistoryTableName, SchemaName];
            if (historyTable == null)
            {
                if (createManagementSchemaIfMissing)
                {
                    historyTable = new Table(database, HistoryTableName, SchemaName);

                    var filenameColumn = new Column(historyTable, HistoryTableFilenameColumnName) { DataType = DataType.NVarChar(256), Nullable = false };
                    historyTable.Columns.Add(filenameColumn);

                    var hashColumn = new Column(historyTable, HistoryTableHashColumnName) { DataType = DataType.NVarChar(128), Nullable = false };
                    historyTable.Columns.Add(hashColumn);

                    var appliedOnColumn = new Column(historyTable, HistoryTableAppliedOnColumnName) { DataType = DataType.DateTime, Nullable = false };
                    historyTable.Columns.Add(appliedOnColumn);

                    var appliedByColumn = new Column(historyTable, HistoryTableAppliedByColumnName) { DataType = DataType.NVarChar(256), Nullable = false };
                    historyTable.Columns.Add(appliedByColumn);

                    historyTable.Create();

                    var primaryKey = new Index(historyTable, HistoryTablePrimaryKeyName);
                    primaryKey.IndexedColumns.Add(new IndexedColumn(primaryKey, filenameColumn.Name));
                    primaryKey.IsUnique = true;
                    primaryKey.IndexKeyType = IndexKeyType.DriPrimaryKey;

                    primaryKey.Create();
                }
                else
                {
                    OnExecutionEvent(ExecutionEventType.Error, "The Gus management schema is invalid.");
                    return false;
                }
            }

            return true;
        }
    }
}
