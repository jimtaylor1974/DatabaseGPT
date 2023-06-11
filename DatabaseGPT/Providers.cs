using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace DatabaseGPT
{
    public class Providers
    {
        public const string SqlClient = "System.Data.SqlClient";

        public static void RegisterDbProviderFactories()
        {
            DbProviderFactories.RegisterFactory(SqlClient, SqlClientFactory.Instance);
        }
    }
}