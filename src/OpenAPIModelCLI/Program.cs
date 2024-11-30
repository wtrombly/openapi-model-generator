using CommandLine;
using Microsoft.Extensions.Logging;
using OpenAPIModelGenerator;

namespace OpenAPIModelCLI;
public class Program
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Path to the OpenAPI spec file.")]
        public required string InputFilePath { get; set; }

        [Option('o', "output", Required = true, HelpText = "Path to the output directory.")]
        public required string OutputDirectory { get; set; }

        [Option('n', "namespace", Default = "CodeGen", HelpText = "Namespace for the generated C# classes.")]
        public string Namespace { get; set; } = "CodeGen";

        [Option('a', "attributes", Required = false, HelpText = "Array of attributes with attribute values")]
        public string? Attributes { get; set; } = null;

        [Option('u', "usings", Required = false, HelpText = "Array of usings sources")]
        public string? Usings { get; set; } = null;

        [Option('x', "xml", Required = false, HelpText = "Does the generated code require xml comments")]
        public bool Documentation { get; set; } = false;
    }

    static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<Options>(args)
            .WithParsedAsync(async options =>
            {
                await ExecuteWorkflowAsync(options);
            });
    }

    private static async Task ExecuteWorkflowAsync(Options options)
    {
        try
        {
            ValidatePaths(options);

            // Set up logging
            using var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger<ModelGenerator>();

            // Initialize and execute the generator
            var generator = new ModelGenerator(
                logger,
                options.InputFilePath,
                options.OutputDirectory,
                options.Namespace,
                options.Attributes,
                options.Usings,
                options.Documentation);

            await generator.Execute();

            Console.WriteLine("Code generation completed successfully!");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
    }

    private static void ValidatePaths(Options options)
    {
        if (!File.Exists(options.InputFilePath))
            throw new FileNotFoundException($"Input file not found at {options.InputFilePath}");

        if (!Directory.Exists(options.OutputDirectory))
            Directory.CreateDirectory(options.OutputDirectory);
    }
}
