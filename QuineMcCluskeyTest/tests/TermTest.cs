using QuineMcCluskey.Commons;
using QuineMcCluskey.Term;
namespace QuineMcCluskeyTest;

[TestClass]
public class TermTest
{
    [TestMethod]
    public void TestTermDiffManual1()
    {
        Term a = new Term(4, new Bit[]{ Bit.F, Bit.F, Bit.F, Bit.F }, 0);
        Term b = new Term(4, new Bit[]{ Bit.F, Bit.F, Bit.F, Bit.F }, 0);
        TermDiff diffCalced = a.Diff(b);
        TermDiff diffGened = new TermDiff(0, new bool[]{ false, false, false, false });
        Assert.AreEqual(diffCalced.diffCount, diffGened.diffCount);
        CollectionAssert.AreEqual(diffCalced.diffs, diffGened.diffs);
    }

    [TestMethod]
    public void TestTermDiffManual2()
    {
        Term a = new Term(4, new Bit[]{ Bit.T, Bit.F, Bit.F, Bit.F }, 0);
        Term b = new Term(4, new Bit[]{ Bit.F, Bit.F, Bit.F, Bit.F }, 0);
        TermDiff diffCalced = a.Diff(b);
        TermDiff diffGened = new TermDiff(1, new bool[]{ true, false, false, false });
        Assert.AreEqual(diffCalced.diffCount, diffGened.diffCount);
        CollectionAssert.AreEqual(diffCalced.diffs, diffGened.diffs);
    }
}
