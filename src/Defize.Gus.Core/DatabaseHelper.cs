namespace Defize.Gus
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using Microsoft.SqlServer.Management.Smo;

    internal class DatabaseHelper
    {
        private const string SchemaName = "Gus";
        private const string HistoryTableName = "GusHistory";
        private const string HistoryTablePrimaryKeyName = "PK_Gus_GusHistory";
        private const string HistoryTableFilenameColumnName = "Filename";
        private const string HistoryTableHashColumnName = "Hash";
        private const string HistoryTableAppliedOnColumnName = "AppliedOn";
        private const string HistoryTableAppliedByColumnName = "AppliedBy";
        private const string RegisterSqlScript = "INSERT INTO [Gus].[GusHistory] ([Filename], [Hash], [AppliedOn], [AppliedBy]) VALUES (N'{0}', N'{1}', GETDATE(), SYSTEM_USER)";

        private readonly GusTaskExecutionContext _context;

        public DatabaseHelper(GusTaskExecutionContext context)
        {
            _context = context;
        }

        public SqlConnection CreateAndOpenConnection(string server)
        {
            _context.RaiseExecutionEvent(string.Format("Connecting to database '{0}'.", server));

            var connectionBuilder = new SqlConnectionStringBuilder { DataSource = server, IntegratedSecurity = true };
            var connectionString = connectionBuilder.ToString();

            var connection = new SqlConnection(connectionString);

            try
            {
                connection.Open();
            }
            catch (SqlException ex)
            {
                _context.RaiseExecutionEvent(ExecutionEventType.Error, string.Format("Unable to open connection to database: {0}", ex.Message));
                return null;
            }

            return connection;
        }

        public Database InitializeDatabase(Server server, string databaseName, bool createDatabaseIfMissing, bool createManagementSchemaIfMissing)
        {
            _context.RaiseExecutionEvent("Initialising database.");

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

        public ICollection<AppliedScript> GetPreviouslyAppliedScripts(Database database)
        {
            var results = database.ExecuteWithResults("SELECT [Filename], [Hash] FROM [Gus].[GusHistory]");
            var table = results.Tables[0];
            var rows = table.Rows;

            return rows.Cast<DataRow>().Select(r => new AppliedScript
                                                           {
                                                               Filename = (string)r[HistoryTableFilenameColumnName],
                                                               Hash = (string)r[HistoryTableHashColumnName]
                                                           }).ToList();
        }

        public void RecordScript(Database database, string filename, string hash)
        {
            filename = filename.Replace("'", "''");
            var sql = string.Format(RegisterSqlScript, filename, hash);
            database.ExecuteNonQuery(sql);
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
                    _context.RaiseExecutionEvent(ExecutionEventType.Error, string.Format("The database '{0}' could not be found.", databaseName));
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
                    _context.RaiseExecutionEvent(ExecutionEventType.Error, "The Gus management schema could not be found.");
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
                    _context.RaiseExecutionEvent(ExecutionEventType.Error, "The Gus management schema is invalid.");
                    return false;
                }
            }

            return true;
        }
    }
}
