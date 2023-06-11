namespace DatabaseGPT.DatabaseSchema;

public class TableMetadata
{
    public TableMetadata()
    {
        Columns = new List<ColumnMetadata>();
    }

    public string Schema { get; set; }
    public string Name { get; set; }
    public object Metadata { get; set; }

    public List<ColumnMetadata> Columns { get; set; }
}