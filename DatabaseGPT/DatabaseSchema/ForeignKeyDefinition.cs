namespace DatabaseGPT.DatabaseSchema;

public class ForeignKeyDefinition
{
    public string primaryKeyColumn { get; set; }
    public string foreignKeyColumn { get; set; }
    public string foreignKeyTable { get; set; }
    public string foreignKeyTableSchema { get; set; }
}