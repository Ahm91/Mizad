using System.Collections.Concurrent;
using System.Reflection;
using Serilog.Core;
using Serilog.Events;

namespace Mizad.Infrastructure.Logging;

/// <summary>
/// When an object is destructured (<c>{@Value}</c>), properties whose names suggest a secret are replaced by
/// <c>***</c>, so passwords and connection strings never reach the log file (D-017, ARCHITECTURE §4.7).
/// </summary>
public sealed class SensitiveDataDestructuringPolicy : IDestructuringPolicy
{
    public const string Mask = "***";

    private static readonly string[] SensitiveNameParts = ["Password", "Secret", "ConnectionString", "Token", "PasswordHash"];

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]?> SensitiveTypes = new();

    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentNullException.ThrowIfNull(propertyValueFactory);

        var properties = SensitiveTypes.GetOrAdd(value.GetType(), GetPropertiesIfSensitive);
        if (properties is null)
        {
            result = null!;
            return false;
        }

        var logProperties = new List<LogEventProperty>(properties.Length);
        foreach (var property in properties)
        {
            LogEventPropertyValue propertyValue = IsSensitive(property.Name)
                ? new ScalarValue(Mask)
                : propertyValueFactory.CreatePropertyValue(property.GetValue(value), destructureObjects: true);
            logProperties.Add(new LogEventProperty(property.Name, propertyValue));
        }

        result = new StructureValue(logProperties, value.GetType().Name);
        return true;
    }

    private static PropertyInfo[]? GetPropertiesIfSensitive(Type type)
    {
        if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
        {
            return null;
        }

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
            .ToArray();

        return properties.Any(p => IsSensitive(p.Name)) ? properties : null;
    }

    private static bool IsSensitive(string propertyName) =>
        SensitiveNameParts.Any(part => propertyName.Contains(part, StringComparison.OrdinalIgnoreCase));
}
