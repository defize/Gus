namespace Defize.Gus
{
    public class ApplyTaskConfiguration
    {
        public string Server { get; set; }
        public string Database { get; set; }
        public bool CreateDatabaseIfMissing { get; set; }
        public bool CreateManagementSchemaIfMissing { get; set; }
        public bool RecordOnly { get; set; }
    }
}
