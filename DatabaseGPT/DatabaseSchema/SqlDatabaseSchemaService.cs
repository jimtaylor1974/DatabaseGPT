using FluentMigrator;

namespace DatabaseGPT.DatabaseSchema
{
    public class SqlDatabaseSchemaService
    {
        private readonly SqlDatabase sqlDatabase;

        public SqlDatabaseSchemaService(SqlDatabase sqlDatabase)
        {
            this.sqlDatabase = sqlDatabase;
        }

        private const string objectsQuery = @"select 
    s.name as [Schema], 
    o.type_desc as [Type],
    o.name as [Name] 
from
    sys.all_objects o
    inner join sys.schemas s on s.schema_id = o.schema_id 
where
    o.type in ('U', 'V') -- tables and views
	and s.name <> 'sys'
	and s.name <> 'INFORMATION_SCHEMA'
	and s.name <> 'VersionInfo'
	and s.name <> 'sysdiagrams'
    and (@SchemaName IS NULL OR s.name = @SchemaName)
order by
    s.name";

        private const string tableColumnsQuery = @"SELECT DISTINCT @ObjectName AS [ObjectName]
	,sys.columns.NAME AS [FieldName]
	,sys.types.NAME AS [DatabaseType]
	,sys.columns.precision AS [NumericPrecision]
	,sys.columns.scale AS [NumericScale]
	,sys.columns.is_nullable AS [IsNullable]
	,(
		SELECT COUNT(column_name)
		FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE
		WHERE TABLE_NAME = sys.tables.NAME
			AND CONSTRAINT_NAME = (
				SELECT constraint_name
				FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
				WHERE TABLE_NAME = sys.tables.NAME
					AND constraint_type = 'PRIMARY KEY'
					AND COLUMN_NAME = sys.columns.NAME
				)
		) AS IsPrimaryKey
	,COLUMNPROPERTY(object_id(@ObjectName), sys.columns.NAME, 'IsIdentity') AS [IsIdentity]
	,(
		SELECT ORDINAL_POSITION
		FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = @ObjectName
			AND TABLE_SCHEMA = @Schema
			AND COLUMN_NAME = sys.columns.NAME
		) AS [Position]
	,(
		SELECT COLUMN_DEFAULT
		FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = @ObjectName
			AND TABLE_SCHEMA = @Schema
			AND COLUMN_NAME = sys.columns.NAME
		) AS [DefaultValue]
	,(
		SELECT CHARACTER_MAXIMUM_LENGTH
		FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = @ObjectName
			AND TABLE_SCHEMA = @Schema
			AND COLUMN_NAME = sys.columns.NAME
		) AS [MaxLength]
	,(
		SELECT sik.keyno
		FROM sysobjects so
		INNER JOIN sysindexes si ON so.id = si.id
		INNER JOIN sysindexkeys sik ON so.id = sik.id
			AND si.indid = sik.indid
		INNER JOIN syscolumns sc ON so.id = sc.id
			AND sik.colid = sc.colid
		WHERE so.xtype = 'u'
			AND (si.STATUS & 32) = 0
			AND (si.STATUS & 2048) = 2048
			AND so.NAME = @ObjectName
			AND sc.NAME = sys.columns.NAME
		) AS [KeyNo]
FROM sys.columns
	,sys.types
	,sys.tables
WHERE sys.tables.object_id = sys.columns.object_id
	AND sys.types.system_type_id = sys.columns.system_type_id
	AND sys.types.user_type_id = sys.columns.user_type_id
	AND sys.tables.NAME = @ObjectName
	AND sys.tables.schema_id = SCHEMA_ID (@Schema)
ORDER BY [Position]";

        private const string viewColumnsQuery = @"SELECT 
 @ObjectName AS [ObjectName]
	,sys.columns.NAME AS [FieldName]
	,sys.types.NAME AS [DatabaseType]
	,sys.columns.precision AS [NumericPrecision]
	,sys.columns.scale AS [NumericScale]
	,sys.columns.is_nullable AS [IsNullable]
	,0 AS IsPrimaryKey
	,COLUMNPROPERTY(object_id(@ObjectName), sys.columns.NAME, 'IsIdentity') AS [IsIdentity]
	,(
		SELECT ORDINAL_POSITION
		FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = @ObjectName
			AND TABLE_SCHEMA = @Schema
			AND COLUMN_NAME = sys.columns.NAME
		) AS [Position]
	,(
		SELECT COLUMN_DEFAULT
		FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = @ObjectName
			AND TABLE_SCHEMA = @Schema
			AND COLUMN_NAME = sys.columns.NAME
		) AS [DefaultValue]
	,(
		SELECT CHARACTER_MAXIMUM_LENGTH
		FROM INFORMATION_SCHEMA.COLUMNS
		WHERE TABLE_NAME = @ObjectName
			AND TABLE_SCHEMA = @Schema
			AND COLUMN_NAME = sys.columns.NAME
		) AS [MaxLength]
	,(
		SELECT sik.keyno
		FROM sysobjects so
		INNER JOIN sysindexes si ON so.id = si.id
		INNER JOIN sysindexkeys sik ON so.id = sik.id
			AND si.indid = sik.indid
		INNER JOIN syscolumns sc ON so.id = sc.id
			AND sik.colid = sc.colid
		WHERE so.xtype = 'u'
			AND (si.STATUS & 32) = 0
			AND (si.STATUS & 2048) = 2048
			AND so.NAME = @ObjectName
			AND sc.NAME = sys.columns.NAME
		) AS [KeyNo]
FROM sys.columns
	,sys.types
,sys.views v
   WHERE sys.columns.object_id = v.object_id
   AND v.[name] = @ObjectName
	AND sys.types.user_type_id = sys.columns.user_type_id";

        private const string foreignKeyQuery = @"SELECT oParent.NAME [ParentTableName]
	,oParentColDtl.TABLE_SCHEMA AS [ParentTableSchema]
	,oParentCol.NAME [ParentColumnName]
	,oReference.NAME [ReferenceTableName]
	,refSchema.name as [ReferenceTableSchema]
	,oReferenceCol.NAME [ReferenceColumnName]
FROM sys.foreign_key_columns FKC
INNER JOIN sys.sysobjects oConstraint ON FKC.constraint_object_id = oConstraint.id
INNER JOIN sys.sysobjects oParent ON FKC.parent_object_id = oParent.id
INNER JOIN sys.all_columns oParentCol ON FKC.parent_object_id = oParentCol.object_id /* ID of the object to which this column belongs.*/
	AND FKC.parent_column_id = oParentCol.column_id /* ID of the column. Is unique within the object.Column IDs might not be sequential.*/
INNER JOIN sys.sysobjects oReference ON FKC.referenced_object_id = oReference.id
INNER JOIN INFORMATION_SCHEMA.COLUMNS oParentColDtl ON oParentColDtl.TABLE_NAME = oParent.NAME
	AND oParentColDtl.COLUMN_NAME = oParentCol.NAME
INNER JOIN sys.all_columns oReferenceCol ON FKC.referenced_object_id = oReferenceCol.object_id /* ID of the object to which this column belongs.*/
	AND FKC.referenced_column_id = oReferenceCol.column_id /* ID of the column. Is unique within the object.Column IDs might not be sequential.*/
INNER JOIN sys.all_objects o on o.object_id = oReference.id
    INNER JOIN sys.schemas refSchema on refSchema.schema_id = o.schema_id 
WHERE oParent.NAME = @ObjectName AND oParentColDtl.TABLE_SCHEMA = @Schema";

        private const string userDefinedTypesQuery = @"select distinct
	user_defined_type.name AS [UserDefinedDatabaseType]
	,t.name AS [DatabaseType]
	,t.precision AS [NumericPrecision]
	,t.scale AS [NumericScale]
from sys.types user_defined_type
inner join sys.types t on t.system_type_id = user_defined_type.system_type_id
where user_defined_type.is_user_defined = 1 and t.is_user_defined = 0 and t.name <> 'sysname'";

        public DatasourceSchemaInfo Import(string providerName, string connectionString, string schemaName = null)
        {
            var databaseSchemaInfo = new DatasourceSchemaInfo();

            var userDefinedTypes = sqlDatabase
                .Query(providerName, connectionString, userDefinedTypesQuery)
                .Select(sqlDatabase.ConvertTo<UserDefinedType>)
                .ToDictionary(key => key.UserDefinedDatabaseType, value => value);

            databaseSchemaInfo.UserDefinedTypes.AddRange(userDefinedTypes.Values);

            var objects = sqlDatabase.Query(providerName, connectionString, objectsQuery, new
            {
                SchemaName = string.IsNullOrWhiteSpace(schemaName) ? null : schemaName
            })
                .Select(sqlDatabase.ConvertTo<DbObject>)
                .ToArray();

            foreach (var dbObject in objects)
            {
                string schema = dbObject.Schema;
                string type = dbObject.Type;
                string name = dbObject.Name;

                ObjectType objectType;

                switch (type)
                {
                    case "USER_TABLE":
                        objectType = ObjectType.Table;
                        break;
                    case "VIEW":
                        objectType = ObjectType.View;
                        break;
                    default:
                        throw new NotSupportedException($"ObjectType of {type} is not supported.");
                }

                var tableDefinition = new TableDefinition
                {
                    schema = schema,
                    objectType = objectType,
                    name = name
                };

                databaseSchemaInfo.TableDefinitions.Add(tableDefinition);

                var columnsQuery = objectType == ObjectType.Table
                    ? tableColumnsQuery
                    : viewColumnsQuery;

                var columns = sqlDatabase.Query(providerName, connectionString, columnsQuery, new
                {
                    ObjectName = name,
                    Schema = schema
                }).Select(sqlDatabase.ConvertTo<DbColumn>).ToArray();

                foreach (var column in columns)
                {
                    bool isPrimaryKey = column.IsPrimaryKey == 1;
                    bool isIdentity = column.IsIdentity == 1;
                    string databaseType = column.DatabaseType;
                    int numericPrecision = column.NumericPrecision;
                    int numericScale = column.NumericScale;

                    if (userDefinedTypes.ContainsKey(column.DatabaseType))
                    {
                        dynamic userDefinedType = userDefinedTypes[column.DatabaseType];

                        databaseType = userDefinedType.DatabaseType;
                        numericPrecision = column.NumericPrecision;
                        numericScale = column.NumericScale;
                    }

                    if (!ScriptUtility.TryGetColumnType(databaseType, out ColumnType columnType))
                    {
                        continue;
                    }

                    var defaultValue = column.DefaultValue;
                    var withDefault = ParseSystemMethodFromDefaultValue(defaultValue);
                    if (withDefault != null)
                    {
                        defaultValue = null;
                    }

                    var columnDefinition = new ColumnDefinition
                    {
                        identity = isIdentity,
                        primaryKey = isPrimaryKey,
                        name = column.FieldName,
                        precision = numericPrecision,
                        size = column.MaxLength,
                        type = columnType.ToString(),
                        nullable = column.IsNullable,
                        defaultValue = defaultValue,
                        withDefault = withDefault
                    };

                    tableDefinition.columns.Add(columnDefinition);
                }
            }

            foreach (var dbObject in objects)
            {
                var entityForeignKeys = sqlDatabase.Query(
                        providerName,
                        connectionString,
                        foreignKeyQuery,
                        new
                        {
                            ObjectName = dbObject.Name, // leftEntity
                            dbObject.Schema
                        })
                    .Select(sqlDatabase.ConvertTo<DbForeignKey>)
                    .ToArray();

                foreach (var entityForeignKey in entityForeignKeys)
                {
                    // System.Diagnostics.Debug.WriteLine(name + " " + entityForeignKey.ToJson());

                    /*
                     * Application
                     * 
		ParentColumnName      "AccountId"
		ParentTableName       "Application"
		ParentTableSchema     "dbo"
		ReferenceColumnName   "Id"
		ReferenceTableName    "Account"
		ReferenceTableSchema  "dbo"
                    */

                    var foreignKey = new ForeignKeyDefinition
                    {
                        primaryKeyColumn = entityForeignKey.ReferenceColumnName,
                        foreignKeyColumn = entityForeignKey.ParentColumnName,
                        foreignKeyTable = entityForeignKey.ParentTableName,
                        foreignKeyTableSchema = entityForeignKey.ParentTableSchema
                    };

                    var foreignTableDefinition =
                        databaseSchemaInfo.TableDefinitions.Single(td =>
                            td.schema == entityForeignKey.ReferenceTableSchema &&
                            td.name == entityForeignKey.ReferenceTableName);

                    foreignTableDefinition.foreignKeys.Add(foreignKey);
                }
            }

            return databaseSchemaInfo;
        }

        private SystemMethods? ParseSystemMethodFromDefaultValue(string defaultValue)
        {
            if (defaultValue == null)
            {
                return null;
            }

            var defaultValueToSystemMethod = new Dictionary<string, SystemMethods>(StringComparer.OrdinalIgnoreCase)
            {
                ["(newid())"] = SystemMethods.NewGuid,
                ["(newsequentialid())"] = SystemMethods.NewSequentialId,
                ["(getdate())"] = SystemMethods.CurrentDateTime,
                ["(sysdatetimeoffset())"] = SystemMethods.CurrentDateTimeOffset,
                ["(getutcdate())"] = SystemMethods.CurrentUTCDateTime,
                ["(user_name())"] = SystemMethods.CurrentUser
            };

            if (defaultValueToSystemMethod.ContainsKey(defaultValue))
            {
                return defaultValueToSystemMethod[defaultValue];
            }

            return null;
        }
    }
}