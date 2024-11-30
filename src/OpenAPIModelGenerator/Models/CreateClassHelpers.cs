using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using OpenAPIModelGenerator.Models.Enums;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenAPIModelGenerator.Models;

public partial class CreateClassHelpers
{
    /// <summary>
    /// Creates a class.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public static ClassDeclarationSyntax CreateClass(string name)
    {
        return ClassDeclaration(Identifier(CreateMemberOrClassName(name)))
            .AddModifiers(Token(SyntaxKind.PublicKeyword));
    }

    /// <summary>
    /// Create class members.
    /// </summary>
    /// <param name="propertyName"></param>
    /// <param name="propertyType"></param>
    /// <param name="description"></param>
    /// <returns></returns>
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

    public static string GetShortDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "";
        }
        description = description.Replace("\n", "");
        description = description.Replace("\r", "");
        description = description.Replace("\t", "");
        return !string.IsNullOrEmpty(description) ? $"{description.Split('.').First()}.".Trim() : "";
    }

    /// <summary>
    /// Uses the array of attributes to add to the class properties.
    /// </summary>
    /// <param name="property"></param>
    /// <param name="attributes"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Creates the class with needed members from the <see cref="OpenApiSchema"/>
    /// If attributes have been included in the method call, these will also be
    /// attached to class members.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="openAPISchema"></param>
    /// <param name="attributes"></param>
    /// <returns></returns>
    public static ClassDeclarationSyntax CreateClassWithMembers(
        string name,
        OpenApiSchema openAPISchema,
        bool _documentation,
        (string attributeName, string attributeValue)[]? attributes = null)
    {
        var classDeclaration = CreateClass(name);

        var properties = openAPISchema.Properties.Select(property =>
        {
            var updatedAttributes = PrepareAttributes(attributes, property);
            var memberName = CreateMemberOrClassName(property.Key);
            var propertyType = GetDataType(property.Value);

            var propertyDeclaration = CreateProperty(memberName, propertyType);
            if (updatedAttributes is not null)
            {
                propertyDeclaration = AddAttributes(propertyDeclaration, attributes!);
            }

            if (_documentation)
            {
                var xmlComments = CreateXmlComments(property);
                propertyDeclaration = AddXmlComments(propertyDeclaration, xmlComments);
            }

            return propertyDeclaration;
        }).ToArray();

        return classDeclaration.AddMembers(properties);
    }

    private static PropertyDeclarationSyntax AddXmlComments(PropertyDeclarationSyntax propertyDeclaration, SyntaxTriviaList xmlComments)
    {
        return propertyDeclaration.WithLeadingTrivia(xmlComments);
    }

    private static SyntaxTriviaList CreateXmlComments(KeyValuePair<string, OpenApiSchema> property)
    {
        var shortDescription = GetShortDescription(property.Value.Description);
        return SyntaxFactory.TriviaList(
             SyntaxFactory.Trivia(
                     SyntaxFactory.DocumentationCommentTrivia(
                         SyntaxKind.SingleLineDocumentationCommentTrivia,
                         List<XmlNodeSyntax>(
                             new XmlNodeSyntax[]{
                                    XmlText()
                                    .WithTextTokens(
                                        TokenList(
                                            XmlTextLiteral(
                                                TriviaList(
                                                    DocumentationCommentExterior("///")),
                                                " ",
                                                " ",
                                                TriviaList()))),
                                    XmlExampleElement(
                                        SingletonList<XmlNodeSyntax>(
                                            XmlText()
                                            .WithTextTokens(
                                                TokenList(
                                                    new []{
                                                        XmlTextLiteral(
                                                            TriviaList(
                                                                DocumentationCommentExterior("///")),
                                                            " ",
                                                            " ",
                                                            TriviaList()),
                                                        XmlTextLiteral(
                                                            shortDescription,
                                                            shortDescription),
                                                        XmlTextLiteral(
                                                            TriviaList(
                                                                DocumentationCommentExterior("///")),
                                                            " ",
                                                            " ",
                                                            TriviaList())}))))
                                    .WithStartTag(
                                        XmlElementStartTag(
                                            XmlName(
                                                Identifier("summary"))))
                                    .WithEndTag(
                                        XmlElementEndTag(
                                            XmlName(
                                                Identifier("summary")))),
                                    XmlText()
                                    .WithTextTokens(
                                        TokenList(
                                            XmlTextNewLine(
                                                TriviaList(),
                                                Environment.NewLine,
                                                Environment.NewLine,
                                                TriviaList())))}))));
    }

    /// <summary>
    /// Prepares attributes array for various attributes that need values that can be generated.
    /// In the case of Json related attributes the property.Key is used to populate appropriate 
    /// values for json property names.
    /// </summary>
    /// <param name="attributes"></param>
    /// <param name="property"></param>
    /// <returns></returns>
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
    /// Creates from a string a valid C# class or member name in Pascal Case.
    /// </summary>
    /// <param name="memberName"></param>
    /// <returns></returns>
    public static string CreateMemberOrClassName(string memberName)
    {
        if (string.IsNullOrWhiteSpace(memberName)) return string.Empty;

        memberName = RegexLibrary.NotAlphaNumeric().Replace(memberName, " ");

        var words = memberName.ToLower().Split([' '], StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < words.Length; i++)
        {
            words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1);
        }

        var result = string.Join(string.Empty, words);

        // Check and remove numbers in beginning of name
        var matches = RegexLibrary.NumbersAndWhiteSpace().Matches(result);
        do
        {
            if (matches.Count > 0 && matches.First().Index == 0)
            {
                result = result.Substring(1, result.Length - 1);
            }
            matches = RegexLibrary.NumbersAndWhiteSpace().Matches(result);
        }
        while (matches.Count > 0 && matches.First().Index == 0);

        // Check that first char is upper case
        var zeroIndexLowerCaseMatch = RegexLibrary.LowerCase().Match(result);
        if (zeroIndexLowerCaseMatch.Success && zeroIndexLowerCaseMatch.Index == 0)
        {
            result = result.Replace(result[0], char.ToUpper(result[0]));
        }

        return result;
    }

    /// <summary>
    /// Gets the appropriate C# data type for the OpenApiSchema property.
    /// </summary>
    /// <param name="propertyType"></param>
    /// <returns></returns>
    public static TypeSyntax GetDataType(OpenApiSchema propertyType)
    {
        if (propertyType.Type.Contains(OpenValueTypes.Number.GetStringValue()) || propertyType.Type.Contains(OpenValueTypes.Integer.GetStringValue()))
        {
            return propertyType.Nullable ? NullableType(PredefinedType(Token(CheckNumberFormat(propertyType.Format)))) : PredefinedType(Token(CheckNumberFormat(propertyType.Format)));
        }
        if (propertyType.Type.Contains(OpenValueTypes.Boolean.GetStringValue()))
        {
            return propertyType.Nullable ? NullableType(PredefinedType(Token(SyntaxKind.BoolKeyword))) : PredefinedType(Token(SyntaxKind.BoolKeyword));
        }
        if (string.Equals(propertyType.Type, OpenValueTypes.Object.GetStringValue()))
        {
            if (propertyType.Reference is not null)
            {
                return propertyType.Nullable ? NullableType(IdentifierName(propertyType.Reference.Id)) : IdentifierName(propertyType.Reference.Id);
            }
            else
            {
                return IdentifierName(OpenValueTypes.Object.GetStringValue());
            }
        }
        if (string.Equals(propertyType.Type, OpenValueTypes.Array.GetStringValue()))
        {
            if (propertyType.Items.Reference is null || propertyType.Items.Type is null)
            {
                return propertyType.Nullable ?
                                    NullableType(ArrayType(IdentifierName(OpenValueTypes.Object.GetStringValue()))
                                    .WithRankSpecifiers(
                                    SingletonList(
                                        ArrayRankSpecifier(
                                            SingletonSeparatedList<ExpressionSyntax>(
                                                OmittedArraySizeExpression()))))) :
                                    ArrayType(IdentifierName(OpenValueTypes.Object.GetStringValue()))
                                    .WithRankSpecifiers(
                                    SingletonList(
                                        ArrayRankSpecifier(
                                            SingletonSeparatedList<ExpressionSyntax>(
                                                OmittedArraySizeExpression()))));
            }
            if (string.Equals(propertyType.Items.Type, OpenValueTypes.Object.GetStringValue()))
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
        if (string.Equals(format, OpenFormatTypes.Int32.GetStringValue(), StringComparison.CurrentCultureIgnoreCase))
        {
            return SyntaxKind.IntKeyword;
        }
        if (string.Equals(format, OpenFormatTypes.Float.GetStringValue(), StringComparison.CurrentCultureIgnoreCase))
        {
            return SyntaxKind.FloatKeyword;
        }
        if (string.Equals(format, OpenFormatTypes.Double.GetStringValue(), StringComparison.CurrentCultureIgnoreCase))
        {
            return SyntaxKind.DoubleKeyword;
        }
        if (string.Equals(format, OpenFormatTypes.Decimal.GetStringValue(), StringComparison.CurrentCultureIgnoreCase))
        {
            return SyntaxKind.DecimalKeyword;
        }
        if (string.Equals(format, OpenFormatTypes.Long.GetStringValue(), StringComparison.CurrentCultureIgnoreCase) ||
            string.Equals(format, "int", StringComparison.CurrentCultureIgnoreCase) ||
            string.Equals(format, OpenFormatTypes.Int64.GetStringValue(), StringComparison.CurrentCultureIgnoreCase))
        {
            return SyntaxKind.LongKeyword;
        }
        return SyntaxKind.DecimalKeyword;
    }
}
