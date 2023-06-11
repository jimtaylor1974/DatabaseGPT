namespace DatabaseGPT.DatabaseSchema;

public class TableDefinition
{
    public TableDefinition()
    {
        objectType = ObjectType.Table;
        columns = new List<ColumnDefinition>();
        foreignKeys = new List<ForeignKeyDefinition>();
    }

    public ObjectType objectType { get; set; }
    public string schema { get; set; }
    public string name { get; set; }
    public List<ColumnDefinition> columns { get; set; }
    public List<ForeignKeyDefinition> foreignKeys { get; set; }
}