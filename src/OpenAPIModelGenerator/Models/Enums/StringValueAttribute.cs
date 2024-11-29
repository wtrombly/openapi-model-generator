
using System.Reflection;

namespace OpenAPIModelGenerator.Models.Enums
{
    [AttributeUsage(AttributeTargets.Field)]
    public class StringValueAttribute(string value) : Attribute
    {
        public string Value { get; } = value;
    }

    public static class AttributeHelper
    {
        public static string GetStringValue(this Enum value)
        {
            Type type = value.GetType();
            string enumName = Enum.GetName(type, value) ?? throw new ArgumentException("Value is not a valid enum constant");
            FieldInfo? field = type.GetField(enumName ?? "");
            StringValueAttribute? attribute = field?.GetCustomAttribute<StringValueAttribute>();
            return attribute != null ? attribute.Value : enumName!;
        }

        public static T? GetEnumValue<T>(string value) where T : Enum
        {
            var enumValues = Enum.GetValues(typeof(T)).Cast<T>().ToList();

            foreach (var enumValue in enumValues)
            {
                if (value.Trim().Equals(enumValue.GetStringValue(), StringComparison.CurrentCultureIgnoreCase))
                {
                    return enumValue;
                }
            }

            return default;
        }
    }
}