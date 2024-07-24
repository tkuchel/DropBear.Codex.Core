# DropBear.Codex.Core Result Types

## Overview

The DropBear.Codex.Core library provides a set of Result types that offer a robust and type-safe way to handle operation outcomes in your C# applications. These types help eliminate null checks, reduce exceptions, and make error handling more explicit and easier to manage.

## Available Result Types

1. `Result`
2. `Result<T>`
3. `Result<TSuccess, TFailure>`
4. `ResultWithPayload<T>`

## Basic Usage

### Result

`Result` is the most basic type, representing success or failure without any associated value.

```csharp
public Result DoSomething()
{
    if (everythingWentWell)
        return Result.Success();
    else
        return Result.Failure("Something went wrong");
}

var result = DoSomething();
if (result.IsSuccess)
    Console.WriteLine("Operation succeeded");
else
    Console.WriteLine($"Operation failed: {result.ErrorMessage}");
```

### Result<T>

`Result<T>` represents an operation that, when successful, returns a value of type T.

```csharp
public Result<int> CalculateValue()
{
    if (canCalculate)
        return Result<int>.Success(42);
    else
        return Result<int>.Failure("Unable to calculate value");
}

var result = CalculateValue();
result.Match(
    onSuccess: value => Console.WriteLine($"Calculated value: {value}"),
    onFailure: (error, _) => Console.WriteLine($"Calculation failed: {error}")
);
```

### Result<TSuccess, TFailure>

`Result<TSuccess, TFailure>` allows you to specify both success and failure types.

```csharp
public Result<int, string> Divide(int a, int b)
{
    if (b == 0)
        return Result<int, string>.Failed("Cannot divide by zero");
    return Result<int, string>.Succeeded(a / b);
}

var result = Divide(10, 2);
var output = result.Match(
    onSuccess: value => $"Result: {value}",
    onFailure: error => $"Error: {error}"
);
Console.WriteLine(output);
```

### ResultWithPayload<T>

`ResultWithPayload<T>` is useful when you need to include additional metadata or when working with serialized data.

```csharp
public ResultWithPayload<UserData> GetUserData(int userId)
{
    var userData = // ... fetch user data
    if (userData != null)
        return ResultWithPayload<UserData>.SuccessWithPayload(userData);
    else
        return ResultWithPayload<UserData>.FailureWithPayload("User not found");
}

var result = GetUserData(123);
if (result.IsValid)
{
    var userData = result.DecompressAndDeserialize().ValueOrThrow();
    Console.WriteLine($"User name: {userData.Name}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

## Advanced Features

### Chaining Operations

You can chain multiple operations using the `Bind` method:

```csharp
Result<int> GetNumber() => Result<int>.Success(10);
Result<int> Double(int n) => Result<int>.Success(n * 2);
Result<string> ToString(int n) => Result<string>.Success(n.ToString());

var result = GetNumber()
    .Bind(Double)
    .Bind(ToString);

// result will be Success("20") if all operations succeed
```

### Transforming Results

Use the `Map` method to transform the success value:

```csharp
var result = GetNumber().Map(n => n.ToString());
// If GetNumber returns Success(10), result will be Success("10")
```

### Error Handling

You can handle errors using the `OnFailure` method:

```csharp
GetNumber()
    .OnFailure((error, ex) => Console.WriteLine($"An error occurred: {error}"))
    .OnSuccess(n => Console.WriteLine($"The number is: {n}"));
```

### Try-Catch Wrapper

The `Try` method provides a convenient way to wrap operations that might throw exceptions:

```csharp
var result = Result<int>.Try(() =>
{
    // Some operation that might throw
    return int.Parse("not a number");
});

// result will be a Failure containing the exception message
```

## Best Practices

1. Prefer using Result types over throwing exceptions for expected failure cases.
2. Use the `Match` method to handle both success and failure cases explicitly.
3. Chain operations using `Bind` and `Map` to create clean and readable code.
4. Use `ResultWithPayload<T>` when you need to include additional metadata or work with serialized data.
5. Leverage the `Try` method to convert exception-throwing code into Result-returning code.

By consistently using these Result types, you can make your code more predictable, easier to reason about, and less prone to runtime errors.
