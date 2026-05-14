# demos

This repository contains .NET demos. The current solution targets .NET 10 and
includes `PropertyTesting/PropertyTesting.csproj`, an MSTest project for
demonstrating property-based testing with
[CsCheck](https://www.nuget.org/packages/CsCheck).

- [CsCheck documentation](https://github.com/AnthonyLloyd/CsCheck/blob/master/README.md)

## OCI references used by the demo

- [OCI Distribution definitions for registry and repository](https://github.com/opencontainers/distribution-spec/blob/dc18cea874b0363a37d64d8a11d9e00293d1e15c/spec.md#L61-L62)
- [OCI Distribution repository `<name>` and tag `<reference>` regexes](https://github.com/opencontainers/distribution-spec/blob/dc18cea874b0363a37d64d8a11d9e00293d1e15c/spec.md#L146-L160)
- [OCI Image descriptor digest grammar](https://github.com/opencontainers/image-spec/blob/13cff54902ec9ad6320cbc487a685b66fcd67171/descriptor.md#L78-L85)
- [OCI Image SHA-256 digest requirement](https://github.com/opencontainers/image-spec/blob/13cff54902ec9ad6320cbc487a685b66fcd67171/descriptor.md#L151-L157)

## Run the tests

```bash
dotnet test
```

To see generated container image examples and a CsCheck classification table in
the console, run the generator output demo with detailed logging:

```bash
dotnet test --filter GeneratorOutputExamples --logger "console;verbosity=detailed"
```
