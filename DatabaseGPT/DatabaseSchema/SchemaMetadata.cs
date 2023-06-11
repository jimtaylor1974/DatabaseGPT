namespace DatabaseGPT.DatabaseSchema;

public class SchemaMetadata
{
    public SchemaMetadata()
    {
        Tables = new List<TableMetadata>();
    }

    public List<TableMetadata> Tables { get; set; }
}