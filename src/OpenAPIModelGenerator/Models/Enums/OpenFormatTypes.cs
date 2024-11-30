namespace OpenAPIModelGenerator.Models.Enums;

/// <summary>
/// Enumeration of Open API format types used to better describe value types and
/// are used to better inform C# type keywords.
/// </summary>
public enum OpenFormatTypes
{
    // numeric
    [StringValue("int32")]
    Int32,
    [StringValue("int64")]
    Int64,
    [StringValue("float")]
    Float,
    [StringValue("double")]
    Double,
    [StringValue("decimal")]
    Decimal,
    [StringValue("long")]
    Long,

    [StringValue("byte")]
    Byte,
    [StringValue("binary")]
    Binary,
    [StringValue("date")]
    Date,
    [StringValue("date-time")]
    DateTime,
}
