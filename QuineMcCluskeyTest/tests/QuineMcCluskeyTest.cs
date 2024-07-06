using QuineMcCluskey;
using QuineMcCluskey.Term;
using Calculator = QuineMcCluskey.QuineMcCluskey;
namespace QuineMcCluskeyTest;

[TestClass]
public class QuineMcCluskeyTest
{
    private static readonly string[] Literals = { "w", "x", "y", "z" };
    private static readonly int[] ManualTerms = { 0, 4, 8, 5, 12, 7, 11, 15 };

    [TestMethod]
    public void FindsOnlyMaximalImplicantsWithCorrectBits()
    {
        var worker = new QuineMcCluskeyWorker(MakeTerms(ManualTerms), 4, Literals);
        var calculator = new Calculator(MakeTerms(ManualTerms), new(), 4, Literals);
        string[] expected = { "w'xy'", "w'xz", "xyz", "wyz", "y'z'" };

        for (int run = 0; run < 2; run++)
        {
            CollectionAssert.AreEqual(expected, worker.PrimeImplicants.Select(t => t.ToVariables(Literals)).ToArray());
            CollectionAssert.AreEqual(expected, calculator.PrimeImplicants.Select(t => t.ToVariables(Literals)).ToArray());
            CollectionAssert.AreEqual(new[] { 8, 8, 1 }, worker.terms.Select(stage => stage.Count).ToArray());
        }
    }

    [TestMethod]
    public void CombinesAllRequiredPrimeImplicants()
    {
        AssertMinimumSops
        (
            ManualTerms, new[] { "w'xz + wyz + y'z'" }
        );
    }

    [TestMethod]
    public void ReturnsAllTiedMinimumCoversWithoutDuplicates()
    {
        AssertMinimumSops(new[] { 0, 1, 2, 5, 6, 7 }, new[]
        {
            "w'x'y' + w'yz' + w'xz",
            "w'x'z' + w'y'z + w'xy"
        });
    }

    [TestMethod]
    public void UsesDontCaresToMergeButDoesNotRequireTheirCoverage()
    {
        AssertMinimumSops(new[] { 0, 1 }, new[] { "w'x'" }, new[] { 2, 3, 7 });
    }

    [TestMethod]
    public void PrefersFewerLiteralsAmongCoversWithEqualTermCounts()
    {
        AssertMinimumSops
        (
            new[] { 1, 5, 7, 12 },
            new[] { "w'y'z + xz + wx" },
            new[] { 8, 13, 14, 15 }
        );
    }

    [TestMethod]
    public void RendersConstantFunctions()
    {
        AssertMinimumSops(Array.Empty<int>(), new[] { "0" });
        AssertMinimumSops(Array.Empty<int>(), new[] { "0" }, new[] { 0, 1 });
        AssertMinimumSops(Enumerable.Range(0, 16).ToArray(), new[] { "1" });
        AssertMinimumSops(new[] { 0 }, new[] { "1" }, Enumerable.Range(1, 15).ToArray());
    }

    private static List<QMCTerm> MakeTerms(IEnumerable<int> ids)
        => ids.Select(id => new QMCTerm(4, id)).ToList();

    private static void AssertMinimumSops(int[] required, string[] expected, int[]? dontCares = null)
    {
        var calculator = new Calculator(MakeTerms(required), MakeTerms(dontCares ?? Array.Empty<int>()), 4, Literals);
        CollectionAssert.AreEquivalent(expected, calculator.RenderMinSOP());

        if (dontCares == null)
        {
            var worker = new QuineMcCluskeyWorker(MakeTerms(required), 4, Literals);
            CollectionAssert.AreEquivalent(expected, worker.RenderMinSOP());
        }
    }
}
