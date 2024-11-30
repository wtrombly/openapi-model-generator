namespace OpenAPIModelGenerator.Models.Enums;

/// <summary>
/// Enumeration of Open API value types.
/// </summary>
public enum OpenValueTypes
{
    [StringValue("integer")]
    Integer,
    [StringValue("number")]
    Number,
    [StringValue("null")]
    Null,
    [StringValue("bool")]
    Boolean,
    [StringValue("string")]
    String,
    [StringValue("object")]
    Object,
    [StringValue("array")]
    Array
}
