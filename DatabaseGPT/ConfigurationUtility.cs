using DatabaseGPT.DatabaseSchema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseGPT;

public class ConfigurationUtility
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<QueryService, QueryService>();
        services.AddScoped<SqlDatabaseSchemaService, SqlDatabaseSchemaService>();
        services.AddScoped<SqlDatabase, SqlDatabase>();
        services.AddScoped<DbConnectionFactory, DbConnectionFactory>();
    }
}