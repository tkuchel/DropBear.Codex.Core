# Result Class Library

This library provides a set of tools for managing operation results in a robust and railway-oriented programming fashion. It offers various result types to encapsulate the outcomes of operations, handling both successes and failures gracefully without throwing exceptions. This approach enhances error handling, makes your codebase cleaner, and improves the maintainability of your applications.

## Features

- **Generic Result Types**: Handle different data types and operations flexibly.
- **Railway-Oriented Programming**: Built-in methods for chaining operations based on success or failure outcomes.
- **Error Handling**: Advanced error handling capabilities without relying on exceptions.
- **Asynchronous Support**: Asynchronous methods to handle I/O-bound and CPU-intensive operations efficiently.

## Installation

To install the Result Class Library, use the following NuGet command:

```bash
dotnet add package DropBear.Codex.Core
```

## Usage Examples

Below are some examples of how to use the various result types provided by the library:

### Using `Result`

```csharp
var result = Result.Success();
if (result.IsSuccess)
{
    Console.WriteLine("Operation succeeded.");
}

var failureResult = Result.Failure("Error occurred.");
failureResult.OnFailure(error => Console.WriteLine(error));
```

### Using `Result<T>`

```csharp
var result = ResultFactory.Success(123);
var nextResult = result.Bind(value => ResultFactory.Success(value.ToString()));
nextResult.OnSuccess(value => Console.WriteLine($"Processed value: {value}"));
```

### Using `Result<TSuccess, TFailure>`

```csharp
var result = ResultFactory.Success<int, string>(42);
result.Match(
    success => Console.WriteLine($"Success with value: {success}"),
    failure => Console.WriteLine($"Failed with error: {failure}")
);
```

### Using `ResultWithPayload<T>`

```csharp
var data = new { Name = "Example", Value = 42 };
var result = ResultFactory.SuccessWithPayload(data);
var deserialized = result.DecompressAndDeserialize();
if (deserialized.IsSuccess)
{
    Console.WriteLine($"Data: {deserialized.Value.Name}");
}
```

## Contributing

Contributions are what make the open-source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

Distributed under the GNU LGPL v3. See [LICENSE](https://www.gnu.org/licenses/lgpl-3.0.en.html) for more information.

## Contact

Terrence Kuchel (DropBear) - Contact me via GitHub.
Project Link: [GitHub](https://github.com/tkuchel/DropBear.Codex.Core)

## Acknowledgements

- Thanks to all contributors who participate in this project.
- Special thanks to those who contribute to railway-oriented programming ideas and patterns.

## Development Status

**Note:** This library is relatively new and under active development. While it is being developed with robustness and best practices in mind, it may still be evolving.

We encourage you to test thoroughly and contribute if possible before using this library in a production environment. The API and features may change as feedback is received and improvements are made. We appreciate your understanding and contributions to making this library better!

Please use the following link to report any issues or to contribute: [GitHub Issues](https://github.com/your_username/your_project/issues).
