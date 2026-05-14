using CsCheck;

namespace PropertyTesting;

[TestClass]
public sealed class SortingProperties
{
    [TestMethod]
    public void SortingPreservesLength()
    {
        Gen.Int.Array.Sample(values =>
        {
            var sorted = values.Order().ToArray();

            return sorted.Length == values.Length;
        });
    }
}
