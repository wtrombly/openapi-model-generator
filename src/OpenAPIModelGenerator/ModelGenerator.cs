//using Microsoft.CodeAnalysis;
//using Microsoft.CodeAnalysis.CSharp;
//using Microsoft.CodeAnalysis.CSharp.Syntax;
//using Microsoft.Extensions.Logging;
//using Microsoft.OpenApi.Models;
//using Microsoft.OpenApi.Readers;
//using OpenAPIModelGenerator.Models;
//using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

//namespace OpenAPIModelGenerator
//{
//    /// <summary>
//    /// Generates simple C# models using open api specifications. Uses the Microsoft.OpenApi library.
//    /// </summary>
//    public class ModelGenerator
//    {
//        /// <summary>
//        /// The input file path for the open api spec file.
//        /// </summary>
//        private readonly string _inputFilePath;

//        /// <summary>
//        /// The output file path for where the new .cs files should be generated.
//        /// </summary>
//        private readonly string _outputFilePath;

//        /// <summary>
//        /// The name space for the generated .cs files.
//        /// </summary>
//        private readonly string _outPutNameSpace;

//        private readonly ILogger _logger;

//        public ModelGenerator(ILogger<ModelGenerator> logger, string inputFilePath, string outputFilePath, string outPutNameSpace = "CodeGen")
//        {
//            if (string.IsNullOrWhiteSpace(inputFilePath))
//                throw new ArgumentException("Input file path cannot be null or empty.", nameof(inputFilePath));
//            if (string.IsNullOrWhiteSpace(outputFilePath))
//                throw new ArgumentException("Output file path cannot be null or empty.", nameof(outputFilePath));

//            _inputFilePath = inputFilePath;
//            _outputFilePath = outputFilePath;
//            _outPutNameSpace = outPutNameSpace;
//            _logger = logger;
//        }

//        /// <summary>
//        /// Method to execute the entire workflow
//        /// </summary>
//        /// <returns></returns>
//        public async Task Execute()
//        {
//            MemberDeclarationSyntax[] members;
//            OpenApiDocument document;
//            try
//            {
//                document = await ReadInputFileAsync();
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Failed to read openApiDocument", ex);
//            }

//            try
//            {
//                members = Compute(document);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Failed to create classes", ex);
//            }

//            try
//            {
//                await WriteOutputFiles(members);
//            }
//            catch (Exception ex)
//            {
//                throw new Exception("Failed to write to files", ex);
//            }
//        }

//        /// <summary>
//        /// Reads and the input file and logs information.
//        /// </summary>
//        /// <returns></returns>
//        /// <exception cref="FileNotFoundException"></exception>
//        private async Task<OpenApiDocument> ReadInputFileAsync()
//        {
//            if (!File.Exists(_inputFilePath))
//                throw new FileNotFoundException($"Input file not found at {_inputFilePath}.");

//            OpenApiDocument document;
//            try
//            {
//                await using var fileStream = File.OpenRead(_inputFilePath);
//                document = new OpenApiStreamReader().Read(fileStream, out var diagnostics);

//                _logger.LogInformation("Document read with the following Spec Version: {SpecVersion}",
//                    diagnostics.SpecificationVersion.ToString());

//                _logger.LogWarning("Warnings Count: {WarningsCount}. This may disrupt code generation.",
//                    diagnostics.Warnings.Count);

//                _logger.LogError("Errors Count: {ErrorsCount}. This may disrupt code generation.",
//                    diagnostics.Errors.Count);


//                if (diagnostics != null)
//                {
//                    foreach (var diagnosticItem in diagnostics.Warnings)
//                    {
//                        _logger.LogWarning("Open API Warning: {diagnosticItem.Message}", diagnosticItem.Message);
//                    }
//                    foreach (var diagnosticItem in diagnostics.Errors)
//                    {
//                        _logger.LogWarning("Open API Error: {diagnosticItem.Message}", diagnosticItem.Message);
//                    }
//                }
//            }
//            catch (Exception ex) 
//            {
//                throw new Exception("Failed to read input file.", ex);
//            }

//            return document;
//        }

//        /// <summary>
//        /// Performs computations or logic on the input data
//        /// </summary>
//        /// <param name="openApiDocument"></param>
//        /// <returns></returns>
//        /// <exception cref="InvalidOperationException"></exception>
//        private static MemberDeclarationSyntax[] Compute(OpenApiDocument openApiDocument)
//        {
//            return openApiDocument.Components.Schemas?.Select(t => CreateClassHelpers.CreateClass(t.Key, t.Value)).ToArray() ?? Array.Empty<MemberDeclarationSyntax>();
//        }

//        /// <summary>
//        /// Writes the computed data to the output directory
//        /// </summary>
//        /// <param name="computedData"></param>
//        private async Task WriteOutputFiles(MemberDeclarationSyntax[] members)
//        {

//            foreach (var member in members)
//            {
//                if (member is ClassDeclarationSyntax classDeclaration)
//                {
//                    // Create the namespace and add the class
//                    var ns = NamespaceDeclaration(ParseName(_outPutNameSpace))
//                        .AddMembers(member);

//                    // Generate the using directive
//                    var usingDirective = UsingDirective(
//                        QualifiedName(
//                            IdentifierName("Newtonsoft"),
//                            IdentifierName("Json")));

//                    // Combine the using directive and namespace into a single compilation unit
//                    var compilationUnit = CompilationUnit()
//                        .AddUsings(usingDirective)
//                        .AddMembers(ns)
//                        .NormalizeWhitespace();

//                    // Generate a unique file name based on the class name
//                    var fileName = $"{classDeclaration.Identifier.Text}.cs";

//                    // Combine directory path with file name
//                    var filePath = Path.Combine(_outputFilePath, fileName);

//                    // Write the result to the file
//                    Directory.CreateDirectory(_outputFilePath);
//                    await using var streamWriter = new StreamWriter(filePath, false);
//                    compilationUnit.WriteTo(streamWriter);
//                }
//            }
//        }
//    }
//}
