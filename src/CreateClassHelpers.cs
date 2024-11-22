using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenAPIModelGenerator.Models;

internal class CreateClassHelpers
{
    public static ClassDeclarationSyntax CreateClass(string name, OpenApiSchema classType) =>
    ClassDeclaration(Identifier(name))
        .AddModifiers(Token(SyntaxKind.PublicKeyword))
        .AddMembers(classType.Properties.Select(property =>
            PropertyDeclaration(
                GetDataType(property.Value), Identifier(ToPascalCase(property.Key)))
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .AddAccessorListAccessors(
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
            )
        .WithAttributeLists(
            SingletonList<AttributeListSyntax>(
                AttributeList(
                    SingletonSeparatedList<AttributeSyntax>(
                        Attribute(
                            IdentifierName("JsonProperty"))
                        .WithArgumentList(
                            AttributeArgumentList(
                                SingletonSeparatedList<AttributeArgumentSyntax>(
                                    AttributeArgument(
                                        LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            Literal(name))))))))))
        ).ToArray());

    private static string ToPascalCase(string input)
    {
        input = Regex.Replace(input, @"[^a-zA-Z0-9\s]", "");

        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        string titleCase = textInfo.ToTitleCase(input.ToLower());

        return titleCase.Replace(" ", "");
    }

    private static TypeSyntax GetDataType(OpenApiSchema propertyType)
    {
        if (propertyType.Type.Contains("int"))
        {
            return propertyType.Nullable ? NullableType(PredefinedType(Token(SyntaxKind.IntKeyword))) : PredefinedType(Token(SyntaxKind.IntKeyword));
        }
        if (propertyType.Type.Contains("number"))
        {
            return propertyType.Nullable ? NullableType(PredefinedType(Token(CheckNumberFormat(propertyType.Format)))) : PredefinedType(Token(CheckNumberFormat(propertyType.Format)));
        }
        if (propertyType.Type.Contains("bool"))
        {
            return propertyType.Nullable ? NullableType(PredefinedType(Token(SyntaxKind.BoolKeyword))) : PredefinedType(Token(SyntaxKind.BoolKeyword));
        }
        if (string.Equals(propertyType.Type, "object"))
        {
            if (propertyType.Reference is not null)
            {
                return propertyType.Nullable ? NullableType(IdentifierName(propertyType.Reference.Id)) : IdentifierName(propertyType.Reference.Id);
            }
            else
            {
                return IdentifierName("object");
            }
        }
        if (string.Equals(propertyType.Type, "array"))
        {
            if (propertyType.Items.Reference is null || propertyType.Items.Type is null)
            {
                return propertyType.Nullable ?
                                    NullableType(ArrayType(IdentifierName("object"))
                                    .WithRankSpecifiers(
                                    SingletonList(
                                        ArrayRankSpecifier(
                                            SingletonSeparatedList<ExpressionSyntax>(
                                                OmittedArraySizeExpression()))))) :
                                    ArrayType(IdentifierName("object"))
                                    .WithRankSpecifiers(
                                    SingletonList(
                                        ArrayRankSpecifier(
                                            SingletonSeparatedList<ExpressionSyntax>(
                                                OmittedArraySizeExpression()))));
            }
            if (string.Equals(propertyType.Items.Type, "object"))
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

    private static SyntaxKind CheckNumberFormat(string format)
    {
        if (string.Equals(format, "float"))
        {
            return SyntaxKind.FloatKeyword;
        }
        if (string.Equals(format, "double"))
        {
            return SyntaxKind.DoubleKeyword;
        }
        if (string.Equals(format, "decimal"))
        {
            return SyntaxKind.DecimalKeyword;
        }
        if (string.Equals(format, "long"))
        {
            return SyntaxKind.LongKeyword;
        }
        return SyntaxKind.DoubleKeyword;
    }
}
