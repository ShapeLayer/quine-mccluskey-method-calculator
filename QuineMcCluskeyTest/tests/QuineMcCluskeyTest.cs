using System.Collections;
using System.Collections.Generic;
using QuineMcCluskey;
using QuineMcCluskey.Commons;
namespace QuineMcCluskeyTest;

[TestClass]
public class QuineMcCluskeyTest
{
    [TestMethod]
    public void TestWorkerGetPrimeImplicantsManual1()
    {
        QuineMcCluskeyWorker worker = new QuineMcCluskeyWorker(
            new List<QMCTerm> {
                new QMCTerm(4, new Bit[]{ Bit.F, Bit.F, Bit.F, Bit.F }, 0),  // A
                new QMCTerm(4, new Bit[]{ Bit.F, Bit.T, Bit.F, Bit.F }, 4),  // B
                new QMCTerm(4, new Bit[]{ Bit.T, Bit.F, Bit.F, Bit.F }, 8),  // C
                new QMCTerm(4, new Bit[]{ Bit.F, Bit.T, Bit.F, Bit.T }, 5),  // D
                new QMCTerm(4, new Bit[]{ Bit.T, Bit.T, Bit.F, Bit.F }, 12), // E
                new QMCTerm(4, new Bit[]{ Bit.F, Bit.T, Bit.T, Bit.T }, 7),  // F
                new QMCTerm(4, new Bit[]{ Bit.T, Bit.F, Bit.T, Bit.T }, 11), // G
                new QMCTerm(4, new Bit[]{ Bit.T, Bit.T, Bit.T, Bit.T }, 15)  // H
            }, 4, new string[] {"w", "x", "y", "z"}
        );
        
        bool areEqual = true;
        List<QMCTerm> implicantsWorked = worker.PrimeImplicants.ToList();
        List<QMCTerm> implicantsManual = new List<QMCTerm>(new QMCTerm[]{
            new QMCTerm(4, new Bit[]{ Bit.F, Bit.X, Bit.F, Bit.F }, new int[]{ 0, 4 }),
            new QMCTerm(4, new Bit[]{ Bit.F, Bit.T, Bit.F, Bit.X }, new int[]{ 4, 5 }),
            new QMCTerm(4, new Bit[]{ Bit.F, Bit.T, Bit.X, Bit.T }, new int[]{ 5, 7 }),
            new QMCTerm(4, new Bit[]{ Bit.X, Bit.T, Bit.T, Bit.T }, new int[]{ 7, 15 }),
            new QMCTerm(4, new Bit[]{ Bit.T, Bit.X, Bit.T, Bit.T }, new int[]{ 11, 15 }),
            new QMCTerm(4, new Bit[]{ Bit.X, Bit.X, Bit.F, Bit.F }, new int[]{ 0, 4, 8, 12 })
        });
        
        for (int i = 0; i < implicantsWorked.Count; i++)
        {
            areEqual = areEqual && (implicantsManual[i].Equals(implicantsWorked[i]));
        }
        Assert.IsTrue(areEqual);
    }
}
