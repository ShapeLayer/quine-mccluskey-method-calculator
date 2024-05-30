using System.Collections;
using System.Collections.Generic;
using QuineMcCluskey;
using QuineMcCluskey.Term;
namespace PrimeImplicant;

public class PrimeImplicant
{
    SortedSet<Term> _minimumSOPs = new SortedSet<Term>();
    public PrimeImplicant(IEnumerable<Term> terms)
    {
        _minimumSOPs = new SortedSet<Term>(terms);
    }

    public void Compute()
    {}
}
