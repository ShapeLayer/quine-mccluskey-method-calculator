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
        Console.WriteLine("Minimum SOP");
        Console.WriteLine(string.Join(", ", worker.GetPrimeImplicants()));
        worker.FindEssentialPrimeImplicants();
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
}
