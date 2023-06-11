namespace DatabaseGPT.Infrastructure.Extensions;

public static class Extensions
{
    public static bool IsSuccessStatusCode(this int statusCode)
    {
        return statusCode >= 200 && statusCode < 300;
    }
}