# demos

This repository contains .NET demos and slide deck sources.

The current .NET solution targets .NET 10 and includes:

- `PropertyTesting/PropertyTesting.csproj`, an MSTest project for demonstrating
  property-based testing with [CsCheck](https://www.nuget.org/packages/CsCheck).

- [CsCheck documentation](https://github.com/AnthonyLloyd/CsCheck/blob/master/README.md)

## Run the tests

```bash
dotnet test
```

## Generate the slide deck PDF

```bash
./build-deck.sh
```

The slide source is `deck.md`. The build script requires
[Pandoc](https://pandoc.org/) and a LaTeX engine such as `tectonic`, `xelatex`,
`lualatex`, or `pdflatex`, and writes `deck.pdf`.
