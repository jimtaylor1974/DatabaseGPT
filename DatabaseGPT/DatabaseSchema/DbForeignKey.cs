namespace DatabaseGPT.DatabaseSchema;

public class DbForeignKey
{
    public string ParentTableSchema { get; set; }
    public string ParentTableName { get; set; }
    public string ParentColumnName { get; set; }
    public string ReferenceTableSchema { get; set; }
    public string ReferenceTableName { get; set; }
    public string ReferenceColumnName { get; set; }
}