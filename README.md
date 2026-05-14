# demos

This repository contains .NET demos. The current solution targets .NET 10 and
includes `PropertyTesting/PropertyTesting.csproj`, an MSTest project for
demonstrating property-based testing with
[CsCheck](https://www.nuget.org/packages/CsCheck).

- [CsCheck documentation](https://github.com/AnthonyLloyd/CsCheck/blob/master/README.md)

## Run the tests

```bash
dotnet test
```

To see generated container image examples and a CsCheck classification table in
the console, run the generator output demo with detailed logging:

```bash
dotnet test --filter GeneratorOutputExamples --logger "console;verbosity=detailed"
```
