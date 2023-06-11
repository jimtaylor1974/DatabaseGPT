namespace DatabaseGPT.DatabaseSchema;

public class UserDefinedType
{
    public string UserDefinedDatabaseType { get; set; }
    public string DatabaseType { get; set; }
    public int NumericPrecision { get; set; }
    public int NumericScale { get; set; }
}