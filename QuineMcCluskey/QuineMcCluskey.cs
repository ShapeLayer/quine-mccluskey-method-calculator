using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuineMcCluskey.Commons;
using QuineMcCluskey.Exceptions;
using QuineMcCluskey.Term;
namespace QuineMcCluskey;

public class QuineMcCluskeyWorker
{
    private List<List<QMCTerm>> _terms = new List<List<QMCTerm>>();
    
    private int _variables;
    private string[] _variableExpressions;

    public List<List<QMCTerm>> terms { get { return _terms; } }
    public int variables { get { return _variables; } }
    public string[] variableExpressions { get { return _variableExpressions; } }
    
    public QuineMcCluskeyWorker
    (
        List<QMCTerm> terms, int variables, string[] variableExpressions
    )
    {
        this._terms = new List<List<QMCTerm>>(){ terms };
        this._variables = variables;
        this._variableExpressions = variableExpressions;
    }

    public SortedSet<QMCTerm> PrimeImplicants
    {
        get
        {
            this.FindPrimeImplicants();
            return this.GetPrimeImplicants();
        }
    }

    public SortedSet<QMCTerm> GetPrimeImplicants()
    {
        SortedSet<QMCTerm> primeImplicants = new SortedSet<QMCTerm>();
        foreach (List<QMCTerm> eachList in this.terms)
        {
            foreach (QMCTerm term in eachList)
            {
                if (term.isActivated)
                {
                    primeImplicants.Add(term);
                }
            }
        }
        return primeImplicants;
    }

    public void Compute()
    {
        this.FindPrimeImplicants();
    }

    public void FindPrimeImplicants()
    {
        int idx = 0;
        while (this.terms.Count > idx)
        {
            List<QMCTerm> now = terms[idx];
            for (int i = 0; i < now.Count; i++)
            {
                for (int j = i + 1; j < now.Count; j++)
                {
                    QMCTerm a = now[i], b = now[j], c = null;
                    if (!a.isActivated && !b.isActivated)
                    {
                        continue;
                    }
                    if (a.Diff(b).diffCount == 1)
                    {
                        c = new QMCTerm(a.Merge(b));
                    }
                    if (c != null)
                    {
                        if (idx + 1 == terms.Count)
                        {
                            this.terms.Add(new List<QMCTerm>());
                        }
                        this.terms[idx + 1].Add(c);
                        a.Deactivate();
                        b.Deactivate();
                    }
                }
            }
            idx++;
        }
    }

    public List<string> RenderMinSOP()
    {
        List<QMCTerm> primeImplicants = this.GetPrimeImplicants().ToList();
        (List<SortedSet<int>> essentialPrimeImplicants, List<bool> isEPIOnce) = this.FindEssentialPrimeImplicants();
        Queue<string> stringBuffer = new Queue<string>();
        for (int i = 0; i < essentialPrimeImplicants.Count; i++)
        {
            if (isEPIOnce[i])
            {
                List<string> rendered = (
                    from epiIdx in essentialPrimeImplicants[i]
                    select primeImplicants[epiIdx].ToVariables(this.variableExpressions)
                ).ToList();
                stringBuffer.Enqueue(string.Join(" + ", rendered));
            }
            else
            {
                for (int j = 0; j < stringBuffer.Count; j++)
                {
                    string popped = stringBuffer.Dequeue();
                    foreach (int epiIdx in essentialPrimeImplicants[i])
                    {
                        stringBuffer.Enqueue(
                            popped + primeImplicants[epiIdx].ToVariables(this.variableExpressions)
                        );
                    }
                }
            }
            SortedSet<int> epis = essentialPrimeImplicants[i];
        }
        return stringBuffer.ToList();
    }

    public (List<SortedSet<int>> essentialPrimeImplicants, List<bool> isEPIOnce) FindEssentialPrimeImplicants()
    {
        this.FindPrimeImplicants();
        return this._FindEssentialPrimeImplicants();
    }

    public (List<SortedSet<int>> essentialPrimeImplicants, List<bool> isEPIOnce) _FindEssentialPrimeImplicants()
    {
        List<QMCTerm> primeImplicants = this.GetPrimeImplicants().ToList();
        List<SortedSet<int>> essentialPrimeImplicants = new List<SortedSet<int>>();
        SortedSet<int> beReduced = new SortedSet<int>();
        List<bool> isEPIOnce = new List<bool>();

        while (beReduced.Count < primeImplicants.Count)
        {
            essentialPrimeImplicants.Add(new SortedSet<int>());
            // Count id includes
            DefaultDictionary<int, SortedSet<int>> idIncludes = new DefaultDictionary<int, SortedSet<int>>();
            for (int i = 0; i < primeImplicants.Count; i++)
            {
                if (beReduced.Contains(i)) continue;
                QMCTerm each = primeImplicants[i];
                foreach (int id in each.IncludeIds)
                {
                    idIncludes[id].Add(i);
                }
            }
            // Find id included only one and term includes that id
            SortedSet<int> includesMinIncluded = new SortedSet<int>();
            IEnumerable<int> includeCounts =
                from kv in idIncludes
                select kv.Value.Count;
            int minIncludeCounts = includeCounts.Min();
            isEPIOnce.Add(minIncludeCounts == 1);
            
            foreach (KeyValuePair<int, SortedSet<int>> kv in idIncludes)
            {
                if (kv.Value.Count == minIncludeCounts) // Min, isOnce
                {
                    int i = kv.Value.First();
                    if (beReduced.Contains(i)) continue;
                    includesMinIncluded.Add(i);
                }
            }
            // Mark that be reduced
            // 이쪽 처리가 다 잘못된듯
            foreach (int i in includesMinIncluded)
            {
                for (int j = 0; j < primeImplicants.Count; j++)
                {
                    if (beReduced.Contains(j)) continue;
                    QMCTerm each = primeImplicants[i];
                    if (each.IncludeIds.Contains(i))
                    {
                        // Add to top
                        essentialPrimeImplicants.Last().Add(j);
                        // Mark to reduced
                        beReduced.Add(j);
                    }
                }
            }
        }
        return (essentialPrimeImplicants, isEPIOnce);
    }

    public override string ToString()
    {
        List<string> termsString = new List<string>();
        foreach (List<QMCTerm> eachTerms in terms)
        {
            List<string> eachTermsString = new List<string>();
            foreach (QMCTerm term in eachTerms)
            {
                eachTermsString.Add(term.ToString());
            }
            termsString.Add(string.Join("\n", eachTermsString));
        }
        return string.Join("\n==============\n", termsString);
    }

}


public class QMCTerm: Term.Term
{
    bool _isActivated = true;
    public bool isActivated { get { return _isActivated; } }
    public void Activate() { this._isActivated = true; }
    public void Deactivate() { this._isActivated = false; }

    public QMCTerm(Term.Term term): base(term.size, term.bits, term.IncludeIds) { }
    protected QMCTerm(int size, Bit[] bits): base(size, bits) {}
    public QMCTerm(int size, Bit[] bits, int id): base(size, bits, id) {}
    public QMCTerm(int size, Bit[] bits, int[] includeIds): base(size, bits, includeIds) {}


    public override string ToString()
    {
        return $"[{(this.isActivated ? "O" : "X")}] {base.ToString()}";
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;
        QMCTerm other = obj as QMCTerm;
        if (other == null) return 1;

        int baseCompared = base.CompareTo(obj);
        return baseCompared;
        if (baseCompared == 0)
            return (this.isActivated ? 1 : 0) - (other.isActivated ? 1 : 0);
        return baseCompared;
    }

    public override bool Equals(object? obj)
    {
        if (obj == null) return false;
        QMCTerm other = obj as QMCTerm;
        if (other == null) return false;
        return this.CompareTo(other) == 0;
    }    
}
