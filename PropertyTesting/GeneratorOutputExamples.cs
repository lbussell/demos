using CsCheck;

namespace PropertyTesting;

[TestClass]
public sealed class GeneratorOutputExamples
{
    private const string LowerAlphaNumeric = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const string Hex = "0123456789abcdef";

    private static readonly Gen<string> Registry =
        Gen.OneOf(
            Gen.Select(
                StringFrom(LowerAlphaNumeric, 3, 12),
                StringFrom(LowerAlphaNumeric, 2, 8),
                (name, tld) => $"{name}.{tld}"),
            Gen.Select(
                StringFrom(LowerAlphaNumeric, 3, 12),
                StringFrom(LowerAlphaNumeric, 2, 8),
                StringFrom(LowerAlphaNumeric, 2, 8),
                (name, middle, tld) => $"{name}.{middle}.{tld}"),
            Gen.Select(
                StringFrom(LowerAlphaNumeric, 3, 12),
                StringFrom(LowerAlphaNumeric, 2, 8),
                Gen.Int[5000, 5999],
                (name, tld, port) => $"{name}.{tld}:{port}"));

    private static readonly Gen<string> RepositoryComponent = StringFrom(LowerAlphaNumeric, 3, 16);

    private static readonly Gen<string> RepositoryName =
        Gen.OneOf(
            RepositoryComponent,
            Gen.Select(
                RepositoryComponent,
                RepositoryComponent,
                (owner, image) => $"{owner}/{image}"),
            Gen.Select(
                RepositoryComponent,
                RepositoryComponent,
                RepositoryComponent,
                (org, team, image) => $"{org}/{team}/{image}"));

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

    private static readonly Gen<string> Sha256DigestFromBytes =
        Sha256DigestBytes.Select(bytes => $"sha256:{Convert.ToHexString(bytes).ToLowerInvariant()}");

    private static readonly Gen<string> Sha256Digest =
        Sha256DigestFromBytes;

    private static readonly Gen<string> Sha256DigestFromStringHelper =
        StringFrom(Hex, 64, 64).Select(hash => $"sha256:{hash}");

    private static readonly Gen<ContainerImage> ContainerImageGen =
        Gen.Select(
            Registry,
            RepositoryName,
            Sha256Digest,
            (registry, repositoryName, digest) => new ContainerImage(
                registry,
                repositoryName,
                digest,
                $"{registry}/{repositoryName}@{digest}"));

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public void GeneratedExamplesCanBeWrittenToTestOutput()
    {
        var registries = Check.Single(Registry.Array[5, 5]);
        var repositoryNames = Check.Single(RepositoryName.Array[5, 5]);
        var digests = Check.Single(Sha256DigestFromStringHelper.Array[3, 3]);
        var containerImages = Check.Single(ContainerImageGen.Array[5, 5]);

        WriteExamples("Registry generator", registries);
        WriteExamples("Repository name generator", repositoryNames);
        WriteExamples("SHA-256 digest generator", digests);
        WriteDigestComparison();

        TestContext.WriteLine("Full image specifier generator");
        foreach (var image in containerImages)
        {
            TestContext.WriteLine($"  registry:   {image.Registry}");
            TestContext.WriteLine($"  repository: {image.RepositoryName}");
            TestContext.WriteLine($"  digest:     {image.Digest}");
            TestContext.WriteLine($"  full spec:  {image.FullSpecifier}");
            TestContext.WriteLine("");
        }
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

                return $"{registryShape}, {repositoryDepth}-component repo";
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
        string Digest,
        string FullSpecifier);
}
