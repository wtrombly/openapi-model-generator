using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using OpenAPIModelGenerator.Models;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenAPIModelGenerator
{
    /// <summary>
    /// Generates simple C# models using open api specifications. Uses the Microsoft.OpenApi library.
    /// </summary>
    public class ModelGenerator
    {
        /// <summary>
        /// The input file path for the open api spec file.
        /// </summary>
        private readonly string _inputFilePath;

        /// <summary>
        /// The output file path for where the new .cs files should be generated.
        /// </summary>
        private readonly string _outputFilePath;

        /// <summary>
        /// The name space for the generated .cs files.
        /// </summary>
        private readonly string _outPutNameSpace;

        /// <summary>
        /// The collection of attributes and values for those attributes.
        /// </summary>
        private readonly string? _attributes;

        /// <summary>
        /// The collection of using directives.
        /// </summary>
        private readonly string? _usings;

        /// <summary>
        /// Denotes whether xml comments are required.
        /// </summary>
        private readonly bool _documentation;

        private readonly ILogger _logger;

        public ModelGenerator(
            ILogger<ModelGenerator> logger,
            string inputFilePath,
            string outputFilePath,
            string outPutNameSpace = "CodeGen",
            string? attributes = null,
            string? usings = null,
            bool documentation = false)
        {
            if (string.IsNullOrWhiteSpace(inputFilePath))
                throw new ArgumentException("Input file path cannot be null or empty.", nameof(inputFilePath));
            if (string.IsNullOrWhiteSpace(outputFilePath))
                throw new ArgumentException("Output file path cannot be null or empty.", nameof(outputFilePath));

            _inputFilePath = inputFilePath;
            _outputFilePath = outputFilePath;
            _outPutNameSpace = outPutNameSpace;
            _attributes = attributes;
            _usings = usings;
            _documentation = documentation;
            _logger = logger;
        }

        /// <summary>
        /// Method to execute the entire workflow
        /// </summary>
        /// <returns></returns>
        public async Task Execute()
        {
            MemberDeclarationSyntax[] members;
            OpenApiDocument document;
            try
            {
                document = await ReadInputFileAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to read openApiDocument", ex);
            }


            (string, string)[] parsedAttributes = [];
            if (_attributes is not null)
            {
                try
                {
                    parsedAttributes = ParseAttributes(_attributes);
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to parse attributes.", ex);
                }
            }

            try
            {
                members = GenerateModels(document, parsedAttributes, _documentation);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create classes", ex);
            }

            try
            {
                await WriteOutputFiles(members, _usings);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to write to files", ex);
            }
        }

        /// <summary>
        /// Splits the csv attributes string into a string[] item into attribute name and attribute value.
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns>
        /// Returns an array of string tuples with name and value respectively.
        /// </returns>
        public static (string, string)[] ParseAttributes(string attributes)
        {
            var splitAttributes = attributes.Split(',');

            return splitAttributes
                .Select(a => a.Split('='))
                .Where(parts => parts.Length == 2)
                .Select(parts => (Key: parts[0], Value: parts[1]))
                .ToArray();
        }

        /// <summary>
        /// Reads and the input file and logs information.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        private async Task<OpenApiDocument> ReadInputFileAsync()
        {
            if (!File.Exists(_inputFilePath))
                throw new FileNotFoundException($"Input file not found at {_inputFilePath}.");

            OpenApiDocument document;
            try
            {
                await using var fileStream = File.OpenRead(_inputFilePath);
                document = new OpenApiStreamReader().Read(fileStream, out var diagnostics);

                _logger.LogInformation("Document read with the following Spec Version: {SpecVersion}",
                    diagnostics.SpecificationVersion.ToString());

                _logger.LogWarning("Warnings Count: {WarningsCount}. This may disrupt code generation.",
                    diagnostics.Warnings.Count);

                _logger.LogError("Errors Count: {ErrorsCount}. This may disrupt code generation.",
                    diagnostics.Errors.Count);


                if (diagnostics != null)
                {
                    foreach (var diagnosticItem in diagnostics.Warnings)
                    {
                        _logger.LogWarning("Open API Warning: {diagnosticItem.Message}", diagnosticItem.Message);
                    }
                    foreach (var diagnosticItem in diagnostics.Errors)
                    {
                        _logger.LogWarning("Open API Error: {diagnosticItem.Message}", diagnosticItem.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to read input file.", ex);
            }

            return document;
        }

        /// <summary>
        /// Generates models based on open-api document component schemas.
        /// </summary>
        /// <param name="openApiDocument"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static MemberDeclarationSyntax[] GenerateModels(OpenApiDocument openApiDocument, (string, string)[] parsedAttributes, bool _documentation)
        {
            return openApiDocument.Components.Schemas?.Select(
                t => CreateClassHelpers.CreateClassWithMembers(
                    t.Key, t.Value, _documentation, [.. parsedAttributes])).ToArray() ??
                    Array.Empty<MemberDeclarationSyntax>();
        }

        /// <summary>
        /// Writes the computed data to the output directory
        /// </summary>
        /// <param name="computedData"></param>
        private async Task WriteOutputFiles(MemberDeclarationSyntax[] members, string? usings)
        {
            foreach (var member in members)
            {
                if (member is ClassDeclarationSyntax classDeclaration)
                {
                    // Create the namespace and add the class
                    var ns = NamespaceDeclaration(ParseName(_outPutNameSpace))
                        .AddMembers(member);

                    var usingsDirectives = new List<UsingDirectiveSyntax>();
                    if (!string.IsNullOrWhiteSpace(usings))
                    {
                        var usingsArray = usings.Split(',');
                        foreach (var us in usingsArray)
                        {
                            var identifierNamesList = us.Split(".").ToList();
                            // Generate the using directive
                            var usingDirective = UsingDirective(
                                QualifiedName(
                                    IdentifierName(identifierNamesList.First()),
                                    IdentifierName(string.Join(".", identifierNamesList.GetRange(1, identifierNamesList.Count - 1)))));
                            usingsDirectives.Add(usingDirective);
                        }
                    }

                    // Combine the using directive and namespace into a single compilation unit
                    var compilationUnit = CompilationUnit()
                        .AddUsings([.. usingsDirectives])
                        .AddMembers(ns)
                        .NormalizeWhitespace();

                    // Generate a unique file name based on the class name
                    var fileName = $"{classDeclaration.Identifier.Text}.cs";

                    // Combine directory path with file name
                    var filePath = Path.Combine(_outputFilePath, fileName);

                    // Write the result to the file
                    Directory.CreateDirectory(_outputFilePath);
                    await using var streamWriter = new StreamWriter(filePath, false);
                    compilationUnit.WriteTo(streamWriter);
                }
            }
        }
    }
}
