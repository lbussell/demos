using CsCheck;
using System.Text.RegularExpressions;

namespace PropertyTesting;

[TestClass]
public sealed class GeneratorOutputExamples
{
    private const string LowerAlphaNumeric = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const string AlphaNumeric = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const string TagRest = AlphaNumeric + "._-";
    private const string Hex = "0123456789abcdef";

    private static readonly Regex OciRepositoryNameRegex = new(
        @"^[a-z0-9]+((\.|_|__|-+)[a-z0-9]+)*(/[a-z0-9]+((\.|_|__|-+)[a-z0-9]+)*)*$",
        RegexOptions.CultureInvariant);

    private static readonly Regex OciTagRegex = new(
        @"^[a-zA-Z0-9_][a-zA-Z0-9._-]{0,127}$",
        RegexOptions.CultureInvariant);

    private static readonly Regex OciSha256DigestRegex = new(
        @"^sha256:[a-f0-9]{64}$",
        RegexOptions.CultureInvariant);

    private static readonly Gen<string> RegistryLabel = StringFrom(LowerAlphaNumeric, 1, 12);

    // OCI Distribution defines "Registry" as a service and discusses a registry hostname
    // with optional port, but the exact OCI regexes are for repository <name> and <reference>.
    private static readonly Gen<string> Registry =
        Gen.OneOf(
            Gen.Select(
                RegistryLabel,
                RegistryLabel,
                (left, right) => $"{left}.{right}"),
            Gen.Select(
                RegistryLabel,
                RegistryLabel,
                RegistryLabel,
                (left, middle, right) => $"{left}.{middle}.{right}"),
            Gen.Select(
                RegistryLabel,
                RegistryLabel,
                Gen.Int[1, 65535],
                (left, right, port) => $"{left}.{right}:{port}"));

    // OCI Distribution repository <name>:
    // https://github.com/opencontainers/distribution-spec/blob/dc18cea874b0363a37d64d8a11d9e00293d1e15c/spec.md#L146-L151
    private static readonly Gen<string> NameAtom = StringFrom(LowerAlphaNumeric, 1, 8);

    private static readonly Gen<string> NameSeparator =
        Gen.OneOf(
            Gen.Const("."),
            Gen.Const("_"),
            Gen.Const("__"),
            Gen.Int[1, 4].Select(length => new string('-', length)));

    private static readonly Gen<string> RepositoryComponent =
        Gen.Select(
            NameAtom,
            Gen.Select(NameSeparator, NameAtom, (separator, atom) => $"{separator}{atom}").Array[0, 3],
            (head, suffixes) => $"{head}{string.Concat(suffixes)}");

    private static readonly Gen<string[]> RepositoryComponents =
        RepositoryComponent.Array[1, 4];

    private static readonly Gen<string> RepositoryName =
        RepositoryComponents.Select(components => string.Join('/', components));

    // OCI Distribution tag <reference>:
    // https://github.com/opencontainers/distribution-spec/blob/dc18cea874b0363a37d64d8a11d9e00293d1e15c/spec.md#L158-L160
    private static readonly Gen<string> Tag =
        Gen.Select(
            Gen.Char[AlphaNumeric + "_"],
            StringFrom(TagRest, 0, 127),
            (first, rest) => $"{first}{rest}");

    private static readonly Gen<int[]> Sha256DigestIndexes =
        Gen.Int[0, Hex.Length - 1].Array[64, 64];

    private static readonly Gen<char[]> Sha256DigestChars =
        Gen.Char[Hex].Array[64, 64];

    private static readonly Gen<byte[]> Sha256DigestBytes =
        Gen.Byte.Array[32, 32];

    private static readonly Gen<string> Sha256DigestFromIntIndexes =
        Sha256DigestIndexes.Select(indices => $"sha256:{new string(indices.Select(index => Hex[index]).ToArray())}");

    private static readonly Gen<string> Sha256DigestFromChars =
        Sha256DigestChars.Select(chars => $"sha256:{new string(chars)}");

    // OCI Image Spec SHA-256 descriptor digest:
    // https://github.com/opencontainers/image-spec/blob/13cff54902ec9ad6320cbc487a685b66fcd67171/descriptor.md#L151-L157
    private static readonly Gen<string> Sha256DigestFromBytes =
        Sha256DigestBytes.Select(bytes => $"sha256:{Convert.ToHexString(bytes).ToLowerInvariant()}");

    private static readonly Gen<string> Sha256Digest =
        Sha256DigestFromBytes;

    private static readonly Gen<ContainerImage> ContainerImageGen =
        Gen.Select(
            Registry,
            RepositoryComponents,
            Tag,
            Sha256Digest,
            (registry, repositoryComponents, tag, digest) =>
            {
                var repositoryName = string.Join('/', repositoryComponents);
                var imageName = repositoryComponents[^1];

                return new ContainerImage(
                    registry,
                    repositoryName,
                    imageName,
                    tag,
                    digest,
                    $"{registry}/{repositoryName}:{tag}",
                    $"{registry}/{repositoryName}@{digest}");
            });

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void GeneratedExamplesCanBeWrittenToTestOutput()
    {
        var registries = Check.Single(Registry.Array[5, 5]);
        var repositoryNames = Check.Single(RepositoryName.Array[5, 5]);
        var tags = Check.Single(Tag.Array[5, 5]);
        var digests = Check.Single(Sha256Digest.Array[3, 3]);
        var containerImages = Check.Single(ContainerImageGen.Array[5, 5]);

        WriteExamples("Registry generator", registries);
        WriteExamples("Repository name generator", repositoryNames);
        WriteExamples("Tag reference generator", tags);
        WriteExamples("SHA-256 digest generator", digests);
        WriteDigestComparison();

        TestContext.WriteLine("Full image specifier generator");
        foreach (var image in containerImages)
        {
            TestContext.WriteLine($"  registry:   {image.Registry}");
            TestContext.WriteLine($"  repository: {image.RepositoryName}");
            TestContext.WriteLine($"  image name: {image.ImageName}");
            TestContext.WriteLine($"  tag:        {image.Tag}");
            TestContext.WriteLine($"  digest:     {image.Digest}");
            TestContext.WriteLine($"  by tag:     {image.TaggedSpecifier}");
            TestContext.WriteLine($"  by digest:  {image.DigestedSpecifier}");
            TestContext.WriteLine("");
        }
    }

    [TestMethod]
    public void GeneratedContainerImagePartsMatchOciDistributionRules()
    {
        ContainerImageGen.Sample(
            image =>
            {
                Assert.IsTrue(OciRepositoryNameRegex.IsMatch(image.RepositoryName));
                Assert.IsTrue(OciTagRegex.IsMatch(image.Tag));
                Assert.IsTrue(OciSha256DigestRegex.IsMatch(image.Digest));
                Assert.IsLessThanOrEqualTo(255, $"{image.Registry}/{image.RepositoryName}".Length);
                Assert.AreEqual(image.RepositoryName.Split('/')[^1], image.ImageName);
                Assert.AreEqual($"{image.Registry}/{image.RepositoryName}:{image.Tag}", image.TaggedSpecifier);
                Assert.AreEqual($"{image.Registry}/{image.RepositoryName}@{image.Digest}", image.DigestedSpecifier);
            },
            iter: 1_000,
            threads: 1);
    }

    [TestMethod]
    public void GeneratedInputDistributionCanBeClassified()
    {
        ContainerImageGen.Sample(
            image =>
            {
                var registryShape = image.Registry.Contains(':', StringComparison.Ordinal)
                    ? "registry with port"
                    : "registry without port";
                var repositoryDepth = image.RepositoryName.Count(c => c == '/') + 1;
                var imageNameShape = image.ImageName.Any(c => c is '.' or '_' or '-')
                    ? "separated image name"
                    : "plain image name";

                return $"{registryShape}, {repositoryDepth}-component repo, {imageNameShape}";
            },
            TestContext.WriteLine,
            iter: 200,
            threads: 1);
    }

    private void WriteExamples(string heading, IEnumerable<string> examples)
    {
        TestContext.WriteLine(heading);
        foreach (var example in examples)
        {
            TestContext.WriteLine($"  {example}");
        }

        TestContext.WriteLine("");
    }

    private void WriteDigestComparison()
    {
        var bytes = Check.Single(Sha256DigestBytes);
        var indexes = Check.Single(Sha256DigestIndexes);
        var chars = Check.Single(Sha256DigestChars);
        var digestFromBytes = $"sha256:{Convert.ToHexString(bytes).ToLowerInvariant()}";
        var digestFromIndexes = $"sha256:{new string(indexes.Select(index => Hex[index]).ToArray())}";
        var digestFromChars = $"sha256:{new string(chars)}";

        TestContext.WriteLine("SHA-256 digest from byte array generator");
        TestContext.WriteLine($"  generated bytes:   [{string.Join(", ", bytes.Take(8))}, ...]");
        TestContext.WriteLine($"  encoded digest:    {digestFromBytes}");
        TestContext.WriteLine("");

        TestContext.WriteLine("SHA-256 digest from int generator");
        TestContext.WriteLine($"  generated indexes: [{string.Join(", ", indexes.Take(16))}, ...]");
        TestContext.WriteLine($"  mapped digest:     {digestFromIndexes}");
        TestContext.WriteLine("");

        TestContext.WriteLine("SHA-256 digest from char generator");
        TestContext.WriteLine($"  generated chars:   {new string(chars.Take(16).ToArray())}...");
        TestContext.WriteLine($"  direct digest:     {digestFromChars}");
        TestContext.WriteLine("");

        WriteExamples(
            "Domain-shaped digests from byte arrays",
            Check.Single(Sha256DigestFromBytes.Array[2, 2]));
        WriteExamples(
            "Text-shaped digests from int-index mapping",
            Check.Single(Sha256DigestFromIntIndexes.Array[2, 2]));
        WriteExamples(
            "Text-shaped digests from direct char generation",
            Check.Single(Sha256DigestFromChars.Array[2, 2]));
    }

    private static Gen<string> StringFrom(string alphabet, int minLength, int maxLength)
    {
        return Gen.Int[0, alphabet.Length - 1].Array[minLength, maxLength]
            .Select(indices => new string(indices.Select(index => alphabet[index]).ToArray()));
    }

    private sealed record ContainerImage(
        string Registry,
        string RepositoryName,
        string ImageName,
        string Tag,
        string Digest,
        string TaggedSpecifier,
        string DigestedSpecifier);
}
