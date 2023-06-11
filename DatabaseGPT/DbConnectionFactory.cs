using System.Data.Common;
using System.Data;

namespace DatabaseGPT;

public class DbConnectionFactory
{
    public IDbConnection Create(string providerName, string connectionString)
    {
        var providerFactory = DbProviderFactories.GetFactory(providerName);

        var connection = providerFactory.CreateConnection();

        connection.ConnectionString = connectionString;

        return connection;
    }
}