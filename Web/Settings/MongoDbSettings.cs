namespace Web.Settings;

public class MongoDbSettings
{
    public string ConnectionString { get; set; }
    public string VectorDatabaseName { get; set; }
    public string SearchIndexName { get; set; }
}