using QuineMcCluskey;
using QuineMcCluskey.Commons;
using QuineMcCluskey.Term;

public class Program
{
    struct ProgramInputFormat
    {
        public int argc;
        public string[] argv;
        public int[] minterms;
    }

    public static void Main(string[] args)
    {
        ProgramInputFormat inputFormat = ReceiveInput();
        List<QMCTerm> qmcTerms = new List<QMCTerm>();
        foreach(int termId in inputFormat.minterms)
        {
            Bit[] newBits = new Bit[inputFormat.argc];
            int n = termId;
            for (int i = 0; i < inputFormat.argc; i++)
            {
                newBits[inputFormat.argc - i - 1] = (n % 2 == 1) ? Bit.T : Bit.F;
                n /= 2;
            }

            qmcTerms.Add
            (
                new QMCTerm
                (
                    inputFormat.argc,
                    newBits,
                    new int[]{ termId }
                )
            );
        }

        QuineMcCluskeyWorker worker = new QuineMcCluskeyWorker(
            qmcTerms, inputFormat.argc, inputFormat.argv
        );

        Console.WriteLine("Essential Prime Implicants");
        List<SortedSet<QMCTerm>> epiList = worker.FindEssentialPrimeImplicants();
        Console.WriteLine
        (
            string.Join(
                ", ",
                epiList[0]
                    .Select(x => x.ToVariables(worker.variableExpressions))
            )
        );
        Console.WriteLine("");

        Console.WriteLine("Minimum SOP");
        List<string> minimumSOPs = worker.RenderMinSOP();
        Console.WriteLine(string.Join("\n", minimumSOPs));
    }


    static ProgramInputFormat ReceiveInput()
    {
        ProgramInputFormat inputFormat = new ProgramInputFormat();
        Console.Write("Enter the number of literals: ");
        inputFormat.argc = int.Parse(Console.ReadLine());
        while (true)
        {
            Console.Write("Enter the literals (must be seperated by space(' ')): ");
            string[] argv = Console.ReadLine().Split(' ');
            if (argv.Length == inputFormat.argc)
            {
                inputFormat.argv = argv;
                break;
            }
            Console.WriteLine($"Error! Length of literals({argv.Length} is not number of literals that entered ({inputFormat.argc}))");
        }
        
        Console.Write("Enter the minterms (must be seperated by space(' ')): ");
        inputFormat.minterms = Console.ReadLine().Split(' ')
            .Select(x => int.Parse(x))
            .ToArray();
        return inputFormat;
    }

}
