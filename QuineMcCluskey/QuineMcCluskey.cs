using System;
using System.Collections.Generic;
using System.Linq;
using QuineMcCluskey.Commons;
using QuineMcCluskey.Term;
namespace QuineMcCluskey;

public class QuineMcCluskey
{
    // this.Terms
    private DefaultDictionary<string, Term.QMCTerm> _terms = new DefaultDictionary<string, Term.QMCTerm>();
    public DefaultDictionary<string, Term.QMCTerm> Terms { get { return _terms; } }

    // this.Requires
    private SortedSet<int> _requires = new SortedSet<int>();
    public IReadOnlySet<int> Requires { get { return (IReadOnlySet<int>)_requires; } }
    public bool IsRequires(int id) => this.Requires.Contains(id);
    public bool ContainsRequires(QMCTerm term)
    {
        foreach (int id in term.IncludeIds)
        {
            if (this.Requires.Contains(id))
                return true;
        }
        return false;
    }
    public bool ContainsRequiresOnly(QMCTerm term)
    {
        bool containsRequiresOnly = true;
        foreach (int id in term.IncludeIds)
        {
            if (!this.Requires.Contains(id))
            {
                containsRequiresOnly = false;
                break;
            }
        }
        return containsRequiresOnly;
    }

    // this.Literals
    private string[] _literals;
    public string[] Literals { get { return this._literals; } }

    // this.DontCares
    private SortedSet<int> _dontCares = new SortedSet<int>();
    public IReadOnlySet<int> DontCares { get { return (IReadOnlySet<int>)_dontCares; } }
    public bool IsDontCare(int id) => this.DontCares.Contains(id);
    public bool ContainsDontCares(QMCTerm term)
    {
        foreach (int id in term.IncludeIds)
        {
            if (this.DontCares.Contains(id))
                return true;
        }
        return false;
    }
    public bool ContainsDontCaresOnly(QMCTerm term)
    {
        bool containsDontCaresOnly = true;
        foreach (int id in term.IncludeIds)
        {
            if (!this.DontCares.Contains(id))
            {
                containsDontCaresOnly = false;
                break;
            }
        }
        return containsDontCaresOnly;
    }

    // this.TotalTerms
    public IReadOnlySet<int> TotalTerms
    {
        get
        {
            SortedSet<int> newSet = new SortedSet<int>();
            newSet.UnionWith(this.Requires);
            newSet.UnionWith(this.DontCares);
            return (IReadOnlySet<int>)newSet;
        }
    }

    // Constructor
    public QuineMcCluskey(
        List<Term.QMCTerm> requires,
        List<Term.QMCTerm> dontCares,
        int literalExpressions,
        string[] literals
    )
    {
        this._literals = literals;
        bool validationPass = true;

        int bitSizes = -1;
        if (requires.Count > 0) bitSizes = requires.First().size;
        else if (dontCares.Count > 0) bitSizes = dontCares.First().size;
        for (int i = 0; i < requires.Count; i++)
        {
            Term.QMCTerm now = requires[i];
            validationPass &= now.size == bitSizes;
            this._terms[now.IdHash] = now;
            foreach(int id in now.IncludeIds)
            {
                this._requires.Add(id);
            }
        }
        for (int i = 0; i < dontCares.Count; i++)
        {
            Term.QMCTerm now = dontCares[i];
            validationPass &= now.size == bitSizes;
            this._terms[now.IdHash] = now;
            foreach(int id in now.IncludeIds)
            {
                this._dontCares.Add(id);
            }
        }

        if (!validationPass) throw new ArgumentException();
    }

    public List<List<QMCTerm>> MergeTerms()
        => MergeTerms(this.Terms.Values.OrderBy(term => term).ToList());

    internal static List<List<QMCTerm>> MergeTerms(List<QMCTerm> initialTerms)
    {
        // reset the flags: callers can run the merge more than once
        foreach (QMCTerm term in initialTerms)
            term.Activate();

        var stages = new List<List<QMCTerm>> { initialTerms };
        for (int stage = 0; stage < stages.Count; stage++)
        {
            List<QMCTerm> current = stages[stage];
            var next = new List<QMCTerm>();
            var mergedIds = new HashSet<string>();
            for (int i = 0; i < current.Count; i++)
            {
                for (int j = i + 1; j < current.Count; j++)
                {
                    QMCTerm a = current[i], b = current[j];
                    TermDiff diff = a.Diff(b);
                    if (!a.CanMerge(b, diff))
                        continue;

                    // merged terms cease to be prime candidates
                    // but can still merge with other terms => deactivate
                    a.Deactivate();
                    b.Deactivate();
                    var merged = new QMCTerm(a.Merge(b, diff));
                    if (mergedIds.Add(merged.IdHash))
                        next.Add(merged);
                }
            }

            if (next.Count > 0)
                stages.Add(next);
        }

        return stages;
    }

    internal static SortedSet<QMCTerm> GetPrimeImplicants(List<List<QMCTerm>> stages)
        => new SortedSet<QMCTerm>(stages.SelectMany(stage => stage).Where(term => term.isActivated));

    public SortedSet<QMCTerm> PrimeImplicants => GetPrimeImplicants(this.MergeTerms());

    public SortedSet<QMCTerm> GetEssentialPrimeImplicants()
    {
        List<QMCTerm> primes = this.PrimeImplicants.ToList();
        var chart = BuildPrimeImplicantChart(primes, this.Requires);
        return new SortedSet<QMCTerm>(chart.Values
            .Where(rows => rows.Count == 1)
            .Select(rows => primes[rows[0]]));
    }

    public List<SortedSet<QMCTerm>> FindEssentialPrimeImplicants()
        => FindMinimumCovers(this.PrimeImplicants, this.Requires);

    private static Dictionary<int, List<int>> BuildPrimeImplicantChart(
        List<QMCTerm> primes, IEnumerable<int> requiredIds)
    {
        // dontcare term
        var chart = requiredIds.Distinct().ToDictionary(id => id, _ => new List<int>());
        for (int i = 0; i < primes.Count; i++)
        {
            foreach (int id in primes[i].IncludeIds)
            {
                if (chart.TryGetValue(id, out var rows))
                    rows.Add(i);
            }
        }

        return chart;
    }

    internal static List<SortedSet<QMCTerm>> FindMinimumCovers(
        SortedSet<QMCTerm> primeImplicants, IEnumerable<int> requiredIds)
    {
        List<QMCTerm> primes = primeImplicants.ToList();
        var chart = BuildPrimeImplicantChart(primes, requiredIds);
        var literalCounts = primes.Select(term => term.bits.Count(bit => bit != Bit.X)).ToArray();

        // Each single-row column forces that row into every valid cover.
        var selected = new SortedSet<int>(chart.Values.Where(rows => rows.Count == 1).Select(rows => rows[0]));
        var uncovered = new HashSet<int>(chart.Keys);
        foreach (int row in selected)
            uncovered.ExceptWith(primes[row].IncludeIds);

        var covers = new List<SortedSet<QMCTerm>>();
        var seenCovers = new HashSet<string>();
        int bestTermCount = int.MaxValue;
        int bestLiteralCount = int.MaxValue;
        Search(uncovered, selected.Sum(row => literalCounts[row]));
        return covers;

        void Search(HashSet<int> remaining, int literalCount)
        {
            // compare complete covers
            // term count => literal count
            if (remaining.Count == 0)
            {
                if (selected.Count > bestTermCount ||
                    (selected.Count == bestTermCount && literalCount > bestLiteralCount))
                    return;

                if (selected.Count < bestTermCount || literalCount < bestLiteralCount)
                {
                    bestTermCount = selected.Count;
                    bestLiteralCount = literalCount;
                    covers.Clear();
                    seenCovers.Clear();
                }

                if (seenCovers.Add(string.Join(",", selected)))
                    covers.Add(new SortedSet<QMCTerm>(selected.Select(row => primes[row])));
                return;
            }

            if (selected.Count >= bestTermCount)
                return;

            // branch (min candidate rows in uncovered minterm)
            int id = remaining.MinBy(id => chart[id].Count);
            foreach (int row in chart[id])
            {
                selected.Add(row);
                var next = new HashSet<int>(remaining);
                next.ExceptWith(primes[row].IncludeIds);
                Search(next, literalCount + literalCounts[row]);
                selected.Remove(row);
            }
        }
    }

    internal static List<string> RenderMinSOP(List<SortedSet<QMCTerm>> covers, string[] literals)
        => covers.Select(cover => cover.Count == 0
            ? "0"
            : string.Join(" + ", cover.Select(term => term.ToVariables(literals)))).ToList();

    public List<string> RenderMinSOP() => RenderMinSOP(this.FindEssentialPrimeImplicants(), this.Literals);
}
