using FluentMigrator;

namespace DatabaseGPT.DatabaseSchema;

public class ColumnDefinition
{
    public string name { get; set; }
    public string type { get; set; }
    public string collationName { get; set; }
    public int? size { get; set; }
    public int? precision { get; set; }
    public bool? nullable { get; set; }
    public bool? notNullable { get; set; }
    public string uniqueIndexName { get; set; }
    public bool? unique { get; set; }
    public string primaryKeyName { get; set; }
    public bool? primaryKey { get; set; }
    public object defaultValue { get; set; }
    public bool? identity { get; set; }
    public string columnDescription { get; set; }
    public SystemMethods? withDefault { get; set; }
    public string indexName { get; set; }
}