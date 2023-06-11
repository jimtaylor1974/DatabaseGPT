using Dapper;
using DatabaseGPT.Infrastructure.Extensions;
using System.Reflection;

namespace DatabaseGPT
{
    public class SqlDatabase
    {
        private readonly DbConnectionFactory dbConnectionFactory;

        public SqlDatabase(DbConnectionFactory dbConnectionFactory)
        {
            this.dbConnectionFactory = dbConnectionFactory;
        }

        public IEnumerable<dynamic> Query(
            string providerName,
            string connectionString,
            string sql,
            object parameters = null)
        {
            var dynamicParameters = GetParameters(parameters);

            using (var connection = dbConnectionFactory.Create(providerName, connectionString))
            {
                connection.Open();
                return connection.Query(sql, dynamicParameters);
            }
        }

        public TResult ConvertTo<TResult>(dynamic result)
        {
            IDictionary<string, object> data = (IDictionary<string, object>)result;
            var properties = typeof(TResult).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (typeof(TResult).IsSimpleType())
            {
                return (TResult)data[data.Keys.First()];
            }

            var instance = Activator.CreateInstance<TResult>();

            foreach (var key in data.Keys)
            {
                var property = properties.FirstOrDefault(p => p.Name.Equals(key, StringComparison.OrdinalIgnoreCase));

                if (property != null)
                {
                    var value = GetValue(data[key]);

                    property.SetValue(instance, value);
                }
            }

            return instance;
        }

        private object GetValue(object value)
        {
            if (value == DBNull.Value)
            {
                return null;
            }

            return value;
        }

        private static DynamicParameters GetParameters(object parameters)
        {
            var dynamicParameters = parameters as DynamicParameters;

            if (dynamicParameters != null)
            {
                return dynamicParameters;
            }

            dynamicParameters = new DynamicParameters();

            var parametersDictionary = parameters as IDictionary<string, object>;

            if (parametersDictionary == null && parameters != null)
            {
                parametersDictionary = parameters.AsDictionary();
            }

            if (parametersDictionary != null)
            {
                foreach (var parameter in parametersDictionary)
                {
                    dynamicParameters.Add(parameter.Key, parameter.Value);
                }
            }

            return dynamicParameters;
        }
    }
}