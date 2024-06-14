using QuineMcCluskey;
using QuineMcCluskey.Commons;
using QuineMcCluskey.Term;

public class Program
{
    struct ProgramInputFormat
    {
        public int argc;
        public string[] argv;
        public int[] terms;
        public int[] dontCares;
    }

    public static void Main(string[] args)
    {
        /* QuineMcCluskey.QuineMcCluskey _worker = new QuineMcCluskey.QuineMcCluskey(
            new List<QMCTerm> {
                new QMCTerm(4, 1),
                new QMCTerm(4, 3),
                new QMCTerm(4, 5),
                new QMCTerm(4, 6),
                new QMCTerm(4, 7),
                new QMCTerm(4, 13),
                new QMCTerm(4, 14),
            },
            new List<QMCTerm> {
                new QMCTerm(4, 8),
                new QMCTerm(4, 10),
                new QMCTerm(4, 12)
            }, 
            4, new string[]{"w", "x", "y", "z"}
        );

        var epi = _worker.FindEssentialPrimeImplicants();
        Console.WriteLine(epi.Count);
        foreach (var each in epi)
        {
            Console.WriteLine(string.Join(", ", each));
        }
        Console.WriteLine(string.Join("\n", _worker.RenderMinSOP()));

        return; */
        ProgramInputFormat inputFormat = ReceiveInput();
        List<QMCTerm> qmcTerms = new List<QMCTerm>();
        foreach(int termId in inputFormat.terms)
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
        List<QMCTerm> qmcDontCares = new List<QMCTerm>();
        foreach(int termId in inputFormat.dontCares)
        {
            Bit[] newBits = new Bit[inputFormat.argc];
            int n = termId;
            for (int i = 0; i < inputFormat.argc; i++)
            {
                newBits[inputFormat.argc - i - 1] = (n % 2 == 1) ? Bit.T : Bit.F;
                n /= 2;
            }

            qmcDontCares.Add
            (
                new QMCTerm
                (
                    inputFormat.argc,
                    newBits,
                    new int[]{ termId }
                )
            );
        }

        QuineMcCluskey.QuineMcCluskey worker = new QuineMcCluskey.QuineMcCluskey(
            qmcTerms, qmcDontCares, inputFormat.argc, inputFormat.argv
        );

        Console.WriteLine("Essential Prime Implicants");
        List<SortedSet<QMCTerm>> epiList = worker.FindEssentialPrimeImplicants();
        Console.WriteLine
        (
            string.Join(
                ", ",
                epiList[0]
                    .Select(x => x.ToVariables(worker.Literals))
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
        
        Console.Write("Enter the terms (must be seperated by space(' ')): ");
        inputFormat.terms = Console.ReadLine().Split(' ')
            .Select(x => int.Parse(x))
            .ToArray();
        Console.Write("Enter the don't cares (must be seperated by space(' ')): ");
        inputFormat.dontCares = Console.ReadLine().Split(' ')
            .Select(x => int.Parse(x))
            .ToArray();
        return inputFormat;
    }

}
