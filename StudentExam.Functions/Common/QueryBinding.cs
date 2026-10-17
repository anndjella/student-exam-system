using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace StudentExam.Functions.Common;

public static class QueryBinding
{
    public static T QueryValue<T>(this HttpRequest request, string name, T fallback)
    {
        if (!request.Query.TryGetValue(name, out var values)) return fallback;
        var value = values.Count > 0 ? values[0] : null;
        if (typeof(T) == typeof(string))
            return string.IsNullOrWhiteSpace(value) ? fallback : (T)(object)value;
        if (string.IsNullOrEmpty(value)) throw new QueryBindingException(name);

        try
        {
            if (typeof(T) == typeof(DateOnly))
                return (T)(object)DateOnly.Parse(value, CultureInfo.InvariantCulture);
            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is FormatException or OverflowException)
        {
            throw new QueryBindingException(name);
        }
    }
}

public sealed class QueryBindingException(string name) : Exception($"Invalid query parameter '{name}'.")
{
    public string ParameterName { get; } = name;
}
