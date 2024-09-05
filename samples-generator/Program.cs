// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Fluid;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.DotNet.Docker.SamplesGenerator;

public class Program
{
    static void Main(string[] args)
    {
        const string OutputDir = "Out";

        ClearDirectory(OutputDir);

        FluidParserOptions options = new() { AllowFunctions = true }; 
        FluidParser fluidParser = new(options);

        SamplesManifest manifest = GetManifest();

        string template = File.ReadAllText("Templates/dotnetapp.liquid");

        TemplateOptions.Default.ValueConverters.Add(v => v is PublishMode publishMode ? $"{publishMode}" : null);
        TemplateOptions.Default.FileProvider = new PhysicalFileProvider(Path.GetFullPath("Templates"));

        if (fluidParser.TryParse(template, out IFluidTemplate? tree, out string error))
        {
            foreach ((string version, List<SampleDockerfile> dockerfiles) in manifest.Versions)
            {
                foreach (var dockerfile in dockerfiles)
                {
                    var context = new TemplateContext(dockerfile);
                    context.SetValue("Version", version);
                    context.SetValue("IsNightly", manifest.IsNightly);

                    string dockerfileContents = tree.Render(context);

                    string outputFilePath = Path.Combine(OutputDir, dockerfile.FileName);
                    File.WriteAllText(outputFilePath, dockerfileContents);
                    Console.WriteLine($"Wrote {outputFilePath}");
                }
            }
        }
        else
        {
            throw new Exception($"Template parsing error: {error}");
        }
    }

    private static void ClearDirectory(string path)
    {
        Console.WriteLine($"Clearing directory {path}...");
        var directoryInfo = new DirectoryInfo(path);

        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }

        foreach (FileInfo file in directoryInfo.GetFiles())
        {
            file.Delete();
        }
    }

    private static SamplesManifest GetManifest()
    {
        return new SamplesManifest()
        {
            IsNightly = true,
            Versions = new()
            {
                {
                    "9.0",
                    [
                        new(
                            "default",
                            PublishMode.FxDependent),
                        new(
                            "self-contained",
                            PublishMode.SelfContained),
                        new(
                            "alpine",
                            PublishMode.FxDependent),
                        new(
                            "alpine-icu",
                            PublishMode.FxDependent,
                            ForceGlobalization: true),
                        new(
                            "debian",
                            PublishMode.FxDependent,
                            ImageSuffix: "bookworm"),
                        new(
                            "ubuntu",
                            PublishMode.FxDependent,
                            ImageSuffix: "noble"),
                        new(
                            "chiseled",
                            PublishMode.FxDependent,
                            ImageSuffix: "noble",
                            IsDistroless: true),
                        new(
                            "azurelinux3.0",
                            PublishMode.FxDependent,
                            ImageSuffix: "azurelinux3.0"),
                        new(
                            "azurelinux3.0-distroless",
                            PublishMode.FxDependent,
                            ImageSuffix: "azurelinux3.0-distroless",
                            IsDistroless: true),
                    ]
                }
            }
        };

    }
}

/*

9.0:
    aspnetapp:
        Name: default
            ImageSuffix: 
            PublishMode: fx-dependent
        Name: self-contained
            ImageSuffix:
            PublishMode: self-contained
        Name: alpine
            ImageSuffix: alpine
            PublishMode: fx-dependent
            ForceGlobalization: true
        Name: ubuntu
            ImageSuffix: noble
            PublishMode: fx-dependent
            ForceGlobalization: true
        Name: ubuntu-chiseled
            ImageSuffix: noble-chiseled
            PublishMode: fx-dependent
            IsDistroless: true

8.0:
    aspnetapp:
        Name: default
            ImageSuffix: 
            PublishMode: fx-dependent
    dotnetapp:
        Name: default
            ImageSuffix: 
            PublishMode: fx-dependent

*/

public class SamplesManifest
{
    public required Dictionary<string, List<SampleDockerfile>> Versions { get; init; }
    public required bool IsNightly { get; init; }
}

public record SampleDockerfile(
    string Name,
    PublishMode PublishMode,
    string ImageSuffix = "",
    bool IsNightly = false,
    bool IsDistroless = false,
    bool ForceGlobalization = false)
{
    public string FileName => Name == "default"
        ? "Dockerfile"
        : "Dockerfile." + Name;
};

public enum PublishMode
{
    FxDependent,
    SelfContained,
    Aot,
}
