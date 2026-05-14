# demos

This repository contains .NET demos. The current solution targets .NET 10 and
includes:

- `PropertyTesting/PropertyTesting.csproj`, an MSTest project for demonstrating
  property-based testing with [CsCheck](https://www.nuget.org/packages/CsCheck).
- `Slides/Slides.csproj`, a console app that generates a single-file HTML slide
  deck with [Fluid](https://github.com/sebastienros/fluid).

- [CsCheck documentation](https://github.com/AnthonyLloyd/CsCheck/blob/master/README.md)

## Run the tests

```bash
dotnet test
```

## Generate the slide deck

```bash
dotnet run --project Slides
```
