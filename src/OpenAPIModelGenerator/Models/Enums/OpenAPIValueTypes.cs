namespace OpenAPIModelGenerator.Models.Enums
{
    public enum OpenAPIValueTypes
    {
        [StringValue("int")]
        Integer,
        [StringValue("float")]
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
}
