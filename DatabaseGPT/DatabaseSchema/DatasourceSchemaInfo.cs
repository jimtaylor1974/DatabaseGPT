namespace DatabaseGPT.DatabaseSchema;

public class DatasourceSchemaInfo
{
    public DatasourceSchemaInfo()
    {
        TableDefinitions = new List<TableDefinition>();
        UserDefinedTypes = new List<UserDefinedType>();
    }

    public List<TableDefinition> TableDefinitions { get; set; }
    public List<UserDefinedType> UserDefinedTypes { get; set; }
}