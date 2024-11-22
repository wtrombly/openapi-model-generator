
# OpenAPI Model Generator

[![NuGet Version](https://img.shields.io/nuget/v/OpenAPIModelGenerator.svg?style=flat-square)](https://www.nuget.org/packages/OpenAPIModelGenerator/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**OpenAPI Model Generator** is a NuGet package designed to simplify generating plain C# models (POCOs) from OpenAPI specifications. Using the **Microsoft.OpenApi** library, this package reads OpenAPI documents, processes them, and outputs well-structured C# classes ready for use in your project.

---

## Features

- **Reads OpenAPI Specs**: Uses `Microsoft.OpenApi.Readers` to parse OpenAPI documents.
- **Generates C# Models**: Converts OpenAPI schemas into C# classes, complete with namespaces and `Newtonsoft.Json` attributes.
- **Highly Configurable**: Specify input/output paths and namespaces for generated files.
- **Built-in Logging**: Leverages `Microsoft.Extensions.Logging` for detailed diagnostics and error reporting.

---

## Installation

Install the package from NuGet:

```sh
dotnet add package OpenAPIModelGenerator
```

---

## Usage

### Example Workflow

Below is a step-by-step guide to use the `ModelGenerator` class in your project:

1. **Initialize the `ModelGenerator`**:
    Provide input and output file paths, along with a logger (e.g., from a dependency injection container).

    ```csharp
    using Microsoft.Extensions.Logging;
    using OpenAPIModelGenerator;

    var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<ModelGenerator>();
    var generator = new ModelGenerator(logger, @"path\to\input.yaml", @"path\to\output", "MyNamespace");
    ```

2. **Execute the Workflow**:
    Call the `Execute` method to process the OpenAPI file and generate models.

    ```csharp
    await generator.Execute();
    ```

3. **Output Files**:
    The generated `.cs` files will be placed in the specified output directory.

---

## API Reference

### `ModelGenerator`

#### Constructor

```csharp
public ModelGenerator(
    ILogger<ModelGenerator> logger, 
    string inputFilePath, 
    string outputFilePath, 
    string outPutNameSpace = "CodeGen")
```

- **`logger`**: An instance of `ILogger<ModelGenerator>` for logging details during execution.
- **`inputFilePath`**: Path to the OpenAPI specification file (YAML or JSON).
- **`outputFilePath`**: Path where the generated `.cs` files will be stored.
- **`outPutNameSpace`** *(optional)*: Namespace for the generated C# models. Defaults to `CodeGen`.

#### Methods

- **`Execute()`**  
   Executes the workflow of reading, computing, and generating models.

- **Private Methods**:
  - `ReadInputFileAsync`: Reads and validates the OpenAPI file.
  - `Compute`: Processes the OpenAPI document and creates class syntax.
  - `WriteOutputFile`: Writes the generated syntax to output files.

---

## Example Output

Here’s an example of a generated class:

```csharp
using Newtonsoft.Json;

namespace MyNamespace
{
    public class SampleModel
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
```

---

## Logging Details

The package provides extensive logging for each stage:
- **Information**: Reads OpenAPI version and diagnostics.
- **Warnings**: Highlights potential issues like warnings in the OpenAPI document.
- **Errors**: Reports critical issues and stops execution.

---

## License

This project is licensed under the [MIT License](LICENSE).  

---

## Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request for enhancements, bug fixes, or feature requests.

---

## Contact

For any questions or feedback, please reach out via the repository's [Issues](https://github.com/yourusername/OpenAPIModelGenerator/issues) section.
