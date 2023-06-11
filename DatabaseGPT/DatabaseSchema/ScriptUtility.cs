namespace DatabaseGPT.DatabaseSchema;

public enum ColumnType
{
    AnsiString,
    Binary,
    Boolean,
    Byte,
    Currency,
    Custom,
    Date,
    DateTime,
    DateTime2,
    DateTimeOffset,
    Decimal,
    Double,
    FixedLengthAnsiString,
    FixedLengthString,
    Float,
    Guid,
    Int16,
    Int32,
    Int64,
    String,
    Time,
    Xml
}

public static class ScriptUtility
{
    // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings

    private static readonly Dictionary<string, ColumnType> databaseTypeMap = new Dictionary<string, ColumnType>
    {
        ["bigint"] = ColumnType.Int64,
        ["binary"] = ColumnType.Binary,
        ["bit"] = ColumnType.Boolean,
        ["char"] = ColumnType.FixedLengthString,
        ["date"] = ColumnType.Date,
        ["datetime"] = ColumnType.DateTime,
        ["datetime2"] = ColumnType.DateTime2,
        ["datetimeoffset"] = ColumnType.DateTimeOffset,
        ["decimal"] = ColumnType.Decimal,
        ["float"] = ColumnType.Float,
        // ["geography"] = ColumnType.Geography,
        // ["geometry"] = ColumnType.Geometry,
        // ["hierarchyid"] = ColumnType.HierarchyId,
        ["image"] = ColumnType.Binary,
        ["int"] = ColumnType.Int32,
        ["money"] = ColumnType.Decimal,
        ["nchar"] = ColumnType.FixedLengthString,
        ["ntext"] = ColumnType.String,
        ["numeric"] = ColumnType.Decimal,
        ["nvarchar"] = ColumnType.String,
        ["real"] = ColumnType.Float,
        ["smalldatetime"] = ColumnType.DateTime,
        ["smallint"] = ColumnType.Int16,
        ["smallmoney"] = ColumnType.Decimal,
        // ["sql_variant"] = ColumnType.Object,
        // ["sysname"] = ColumnType.SysName,
        ["text"] = ColumnType.String,
        ["time"] = ColumnType.Time,
        ["timestamp"] = ColumnType.Binary,
        ["tinyint"] = ColumnType.Byte,
        ["uniqueidentifier"] = ColumnType.Guid,
        ["varbinary"] = ColumnType.Binary,
        ["varchar"] = ColumnType.String,
        ["xml"] = ColumnType.Xml
    };

    public static bool TryGetColumnType(string databaseType, out ColumnType columnType)
    {
        return databaseTypeMap.TryGetValue(databaseType, out columnType);
    }
}