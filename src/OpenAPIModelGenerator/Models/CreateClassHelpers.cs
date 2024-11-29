using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenAPIModelGenerator.Models.Enums;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenAPIModelGenerator.Models;

public partial class CreateClassHelpers
{

    [GeneratedRegex(@"[a-z]")]
    private static partial Regex LowerCase();
    [GeneratedRegex(@"[0-9\s]")]
    private static partial Regex NumbersAndWhiteSpace();
    [GeneratedRegex(@"[^a-zA-Z0-9]")]
    private static partial Regex NotAlphaNumeric();

    public static ClassDeclarationSyntax CreateClass(string name)
    {
        return ClassDeclaration(Identifier(name))
            .AddModifiers(Token(SyntaxKind.PublicKeyword));
    }

    public static PropertyDeclarationSyntax CreateProperty(string propertyName, TypeSyntax propertyType)
    {
        return PropertyDeclaration(propertyType, Identifier(propertyName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
    }

    public static PropertyDeclarationSyntax AddAttributes(PropertyDeclarationSyntax property, params (string attributeName, string? attributeValue)[] attributes)
    {
        foreach (var (attributeName, attributeValue) in attributes)
        {
            if (attributeValue != null)
            {
                property = property.WithAttributeLists(
                                property.AttributeLists.Add(
                                    AttributeList(
                                        SingletonSeparatedList<AttributeSyntax>(
                                            Attribute(
                                                IdentifierName(attributeName))
                                            .WithArgumentList(
                                                AttributeArgumentList(
                                                    SingletonSeparatedList<AttributeArgumentSyntax>(
                                                        AttributeArgument(
                                                            LiteralExpression(
                                                                SyntaxKind.StringLiteralExpression,
                                                                Literal(attributeValue))))))))));
            }
            else
            {
                property = property.WithAttributeLists(
                property.AttributeLists.Add(
                    AttributeList(
                        SingletonSeparatedList<AttributeSyntax>(
                            Attribute(
                                IdentifierName(attributeName))
                        ))));
            }
        }

        return property;
    }

    public static ClassDeclarationSyntax CreateClassWithMembers(
        string name,
        OpenApiSchema classType,
        (string attributeName, string attributeValue)[]? attributes = null)
    {
        var classDeclaration = CreateClass(name);

        var properties = classType.Properties.Select(property =>
        {
            var updatedAttributes = PrepareAttributes(attributes, property);
            var propertyName = CreateMemberOrClassName(property.Key);
            var propertyType = GetDataType(property.Value);

            var propertyDeclaration = CreateProperty(propertyName, propertyType);

            if (updatedAttributes is not null)
            {
                propertyDeclaration = AddAttributes(propertyDeclaration, attributes!);
            }

            return propertyDeclaration;
        }).ToArray();

        return classDeclaration.AddMembers(properties);
    }

    private static (string attributeName, string attributeValue)[]? PrepareAttributes(
        (string name, string value)[]? attributes,
        KeyValuePair<string, OpenApiSchema> property)
    {
        if (attributes is null)
        {
            return null;
        }

        for (var i = 0; i < attributes.Length; i++)
        {
            if (string.Equals(attributes[i].name, nameof(Newtonsoft.Json.JsonPropertyAttribute).Replace("Attribute", "")) ||
                    string.Equals(attributes[i].name, nameof(System.Text.Json.Serialization.JsonPropertyNameAttribute).Replace("Attribute", "")))
            {
                attributes[i].value = property.Key;
            }
        }

        return attributes;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="propertyName"></param>
    /// <returns></returns>
    public static string CreateMemberOrClassName(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName)) return string.Empty;

        propertyName = NotAlphaNumeric().Replace(propertyName, " ");

        var words = propertyName.ToLower().Split([' '], StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length; i++)
        {
            words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
        }

        var result = string.Join(string.Empty, words);

        // Check and remove numbers in beginning of name
        var matches = NumbersAndWhiteSpace().Matches(result);
        do
        {
            if (matches.Count > 0 && matches.First().Index == 0)
            {
                result = result.Substring(1, result.Length - 1);
            }
            matches = NumbersAndWhiteSpace().Matches(result);
        }
        while (matches.Count > 0 && matches.First().Index == 0);

        // Check that first char is upper case
        var zeroIndexLowerCaseMatch = LowerCase().Match(result);
        if (zeroIndexLowerCaseMatch.Success && zeroIndexLowerCaseMatch.Index == 0)
        {
            result = result.Replace(result[0], char.ToUpper(result[0]));
        }

        return result;
    }

    public static TypeSyntax GetDataType(OpenApiSchema propertyType)
    {
        if (propertyType.Type.Contains(OpenAPIValueTypes.Number.GetStringValue()) || propertyType.Type.Contains(OpenAPIValueTypes.Integer.GetStringValue()))
        {
            return propertyType.Nullable ? NullableType(PredefinedType(Token(CheckNumberFormat(propertyType.Format)))) : PredefinedType(Token(CheckNumberFormat(propertyType.Format)));
        }
        if (propertyType.Type.Contains(OpenAPIValueTypes.Boolean.GetStringValue()))
        {
            return propertyType.Nullable ? NullableType(PredefinedType(Token(SyntaxKind.BoolKeyword))) : PredefinedType(Token(SyntaxKind.BoolKeyword));
        }
        if (string.Equals(propertyType.Type, OpenAPIValueTypes.Object.GetStringValue()))
        {
            if (propertyType.Reference is not null)
            {
                return propertyType.Nullable ? NullableType(IdentifierName(propertyType.Reference.Id)) : IdentifierName(propertyType.Reference.Id);
            }
            else
            {
                return IdentifierName(OpenAPIValueTypes.Object.GetStringValue());
            }
        }
        if (string.Equals(propertyType.Type, OpenAPIValueTypes.Array.GetStringValue()))
        {
            if (propertyType.Items.Reference is null || propertyType.Items.Type is null)
            {
                return propertyType.Nullable ?
                                    NullableType(ArrayType(IdentifierName(OpenAPIValueTypes.Object.GetStringValue()))
                                    .WithRankSpecifiers(
                                    SingletonList(
                                        ArrayRankSpecifier(
                                            SingletonSeparatedList<ExpressionSyntax>(
                                                OmittedArraySizeExpression()))))) :
                                    ArrayType(IdentifierName(OpenAPIValueTypes.Object.GetStringValue()))
                                    .WithRankSpecifiers(
                                    SingletonList(
                                        ArrayRankSpecifier(
                                            SingletonSeparatedList<ExpressionSyntax>(
                                                OmittedArraySizeExpression()))));
            }
            if (string.Equals(propertyType.Items.Type, OpenAPIValueTypes.Object.GetStringValue()))
            {
                return propertyType.Nullable ?
                    NullableType(ArrayType(IdentifierName(propertyType.Items.Reference.Id))
                    .WithRankSpecifiers(
                    SingletonList(
                        ArrayRankSpecifier(
                            SingletonSeparatedList<ExpressionSyntax>(
                                OmittedArraySizeExpression()))))) :
                    ArrayType(IdentifierName(propertyType.Items.Reference.Id))
                    .WithRankSpecifiers(
                    SingletonList(
                        ArrayRankSpecifier(
                            SingletonSeparatedList<ExpressionSyntax>(
                                OmittedArraySizeExpression()))));
            }
            return propertyType.Nullable ? NullableType(ArrayType(IdentifierName(propertyType.Items.Type))) : ArrayType(IdentifierName(propertyType.Items.Type));
        }
        return propertyType.Nullable ? NullableType(NullableType(PredefinedType(Token(SyntaxKind.StringKeyword)))) : PredefinedType(Token(SyntaxKind.StringKeyword));
    }

    /// <summary>
    /// Checks for typical open api formats for number types and returns the appropriate syntax kind.
    /// </summary>
    /// <param name="format"></param>
    /// <returns>
    /// A <see cref="SyntaxKind"/> value representing the appropriate type for the given format,
    /// or <see cref="SyntaxKind.ObjectKeyword"/> if the format is not recognized.
    /// </returns>
    public static SyntaxKind CheckNumberFormat(string format)
    {
        if (string.Equals(format, "int32", StringComparison.CurrentCultureIgnoreCase))
        {
            return SyntaxKind.IntKeyword;
        }
        if (string.Equals(format, "float", StringComparison.CurrentCultureIgnoreCase))
        {
            return SyntaxKind.FloatKeyword;
        }
        if (string.Equals(format, "double", StringComparison.CurrentCultureIgnoreCase))
        {
            return SyntaxKind.DoubleKeyword;
        }
        if (string.Equals(format, "decimal", StringComparison.CurrentCultureIgnoreCase))
        {
            return SyntaxKind.DecimalKeyword;
        }
        if (string.Equals(format, "long", StringComparison.CurrentCultureIgnoreCase) ||
            string.Equals(format, "int", StringComparison.CurrentCultureIgnoreCase) ||
            string.Equals(format, "int64", StringComparison.CurrentCultureIgnoreCase))
        {
            return SyntaxKind.LongKeyword;
        }
        return SyntaxKind.DecimalKeyword;
    }
}
