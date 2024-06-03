using QuineMcCluskey;
using QuineMcCluskey.Commons;
using QuineMcCluskey.Term;

public class Program
{
    public static void Main(string[] args)
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
        Console.WriteLine("Before ===========");
        Console.WriteLine(worker.ToString());
        worker.Compute();
        Console.WriteLine("After ============");
        Console.WriteLine(worker.ToString());
        Console.WriteLine("Prime Implicants");
        Console.WriteLine(string.Join("\n", worker.GetPrimeImplicants()));
        worker.FindEssentialPrimeImplicants();
        TestWorkerGetPrimeImplicantsManual1();
    }

    static void ReceiveInput()
    {
        Console.Write("Enter the number of variables: ");
        int n = int.Parse(Console.ReadLine());
        Console.Write("Enter the variables: ");
        string[] variables = Console.ReadLine().Split(' ');
        Console.Write("Enter the minterms: ");
        int[] minterms = (
            from x in Console.ReadLine().Split(' ')
            select int.Parse(x)
        ).ToArray();
    }

    public static void TestWorkerGetPrimeImplicantsManual1()
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
            Console.WriteLine($"{i} {areEqual} {implicantsManual[i]} {implicantsWorked[i]}");
        }
    }

}
