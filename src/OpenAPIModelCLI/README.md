
# OpenAPI Model Generator CLI

[![NuGet Version](https://img.shields.io/nuget/v/OpenAPIModelGenerator.svg?style=flat-square)](https://www.nuget.org/packages/OpenAPIModelCLI/)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

**OpenAPI Model Generator CLI** is a lightweight command-line interface for generating plain C# models (POCOs) from OpenAPI specifications. It leverages the **OpenAPIModelGenerator** library to streamline development workflows.

---

## Features

- **Generate Models Quickly**: Turn OpenAPI schemas into C# models in seconds.
- **Custom Output Settings**: Specify output directory and namespace for generated files.
- **Comprehensive Logs**: Monitor each step of the process via the console.
- **Platform Agnostic**: Works seamlessly on Windows, macOS, and Linux.

---

## Installation

Install the CLI globally using the .NET CLI:

```sh
dotnet tool install -g openapi-gen
```

To update to the latest version:

```sh
dotnet tool update -g openapi-gen
```

---

## Usage

### Command Syntax

```sh
openapi-gen -i <InputFilePath> -o <OutputDirectory> -n <Namespace>
```

### Options

| Option                 | Description                                 | Required | Default      |
|------------------------|---------------------------------------------|----------|--------------|
| `-i`, `--input`        | Path to the OpenAPI specification file      | Yes      | N/A          |
| `-o`, `--output`       | Directory for the generated C# files        | Yes      | N/A          |
| `-n`, `--namespace`    | Namespace for the generated files           | No       | `CodeGen`    |
| `-h`, `--help`         | Displays the help message                   | No       | N/A          |

---

### Examples

#### Generate Models with Custom Namespace

```sh
openapi-gen -i ./openapi.yaml -o ./GeneratedModels -n MyCustomNamespace
```

#### Generate Models with Default Namespace

```sh
openapi-gen -i ./openapi.json -o ./Models
```

#### Display Help

```sh
openapi-gen --help
```

---

## Example Output

Here’s an example of a generated class:

```csharp
using Newtonsoft.Json;

namespace MyCustomNamespace
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

## Logging

The CLI provides real-time logs to ensure transparency and traceability:
- **INFO**: Indicates the progress of the operation.
- **WARNING**: Alerts about potential issues in the input file.
- **ERROR**: Stops execution for critical errors.

### Example Log Output

```sh
[INFO] Reading OpenAPI specification from ./openapi.yaml
[INFO] OpenAPI version detected: 3.1.0
[INFO] Generating models in ./GeneratedModels
[INFO] Successfully generated 5 models!
```

---

## Troubleshooting

### Common Issues

1. **Invalid Input File Path**:
   Ensure the file path provided via `--input` exists and is accessible.

2. **Permission Issues**:
   Verify you have write permissions for the output directory.

3. **Invalid OpenAPI Document**:
   Use an OpenAPI validator to confirm the document adheres to the OpenAPI specification.

---

## License

This project is licensed under the [MIT License](LICENSE).

---

## Contributing

We welcome contributions! If you'd like to report an issue, request a feature, or submit a pull request, please visit the [GitHub repository](https://github.com/yourusername/OpenAPIModelGeneratorCLI).

---

## Contact

For questions or support, please reach out via the repository's [Issues](https://github.com/yourusername/OpenAPIModelGeneratorCLI/issues) section.

---
