namespace DatabaseGPT.Infrastructure.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsSimpleType(this Type type)
        {
            if (type.IsNullable())
            {
                // nullable type, check if the nested type is simple.
                return IsSimpleType(type.GetGenericArguments()[0]);
            }

            return type.IsSubclassOf(typeof(ValueType)) || type == typeof(string) || type == typeof(DateTime);
        }
    }
}