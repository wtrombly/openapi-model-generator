
using System.Reflection;

namespace OpenAPIModelGenerator.Models.Enums;

/// <summary>
/// Adding a string value attribute.
/// </summary>
/// <param name="value"></param>
[AttributeUsage(AttributeTargets.Field)]
public class StringValueAttribute(string value) : Attribute
{
    public string Value { get; } = value;
}

/// <summary>
/// Helper class for getting enum values.
/// </summary>
public static class AttributeHelper
{
    /// <summary>
    /// Gets string value from enum that has attribute value set.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static string GetStringValue(this Enum value)
    {
        Type type = value.GetType();
        string enumName = Enum.GetName(type, value) ?? throw new ArgumentException("Value is not a valid enum constant");
        FieldInfo? field = type.GetField(enumName ?? "");
        StringValueAttribute? attribute = field?.GetCustomAttribute<StringValueAttribute>();
        return attribute != null ? attribute.Value : enumName!;
    }
}