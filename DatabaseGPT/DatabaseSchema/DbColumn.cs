namespace DatabaseGPT.DatabaseSchema;

public class DbColumn
{
    public string ObjectName { get; set; }
    public string FieldName { get; set; }
    public string DatabaseType { get; set; }
    public int NumericPrecision { get; set; }
    public int NumericScale { get; set; }
    public bool IsNullable { get; set; }
    public int IsPrimaryKey { get; set; }
    public int? IsIdentity { get; set; }
    public int Position { get; set; }
    public string DefaultValue { get; set; }
    public int? MaxLength { get; set; }
    public short? KeyNo { get; set; }
}