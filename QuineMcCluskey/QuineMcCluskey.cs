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
        List<SortedSet<QMCTerm>> essentialPrimeImplicants = this.FindEssentialPrimeImplicants();
        Queue<string> stringBuffer = new Queue<string>();
        foreach(SortedSet<QMCTerm> termSet in essentialPrimeImplicants)
        {
            stringBuffer.Enqueue(
                string.Join(" + ",
                    termSet
                        .Select(term => term.ToVariables(this.variableExpressions))
                )
            );
        }
        
        return stringBuffer.ToList();
    }

    public List<SortedSet<QMCTerm>> FindEssentialPrimeImplicants()
    {
        this.FindPrimeImplicants();
        return this._FindEssentialPrimeImplicants();
    }

    public List<SortedSet<QMCTerm>> _FindEssentialPrimeImplicants()
    {
        List<QMCTerm> primeImplicants = this.GetPrimeImplicants().ToList();
        List<SortedSet<int>> epiListIdxSets = new List<SortedSet<int>>();
        List<bool> epiMinCountIs1 = new List<bool>();
        List<SortedSet<int>> epiMin = new List<SortedSet<int>>();
        SortedSet<int> reducedTermId = new SortedSet<int>();
        SortedSet<int> reducedListIdx = new SortedSet<int>();
        
        int totalTermIdCount = new SortedSet<int> (primeImplicants
            .SelectMany(term => term.IncludeIds)
        ).Count;

        while (reducedTermId.Count < totalTermIdCount)
        {
            // init
            epiListIdxSets.Add(new SortedSet<int>());
            epiMin.Add(new SortedSet<int>());
            epiMinCountIs1.Add(false);

            // Count each term id exists
            DefaultDictionary<int, SortedSet<int>> idIncludes = new DefaultDictionary<int, SortedSet<int>>();
            for (int listIdx = 0; listIdx < primeImplicants.Count; listIdx++)
            {
                QMCTerm each = primeImplicants[listIdx];
                foreach (int termId in each.IncludeIds)
                    if (!reducedTermId.Contains(termId))                
                        idIncludes[termId].Add(listIdx);
            }

            // Find Minimum Count that Each Valid Term
            int minIncludeCounts = idIncludes
                .Where(kv => !reducedTermId.Contains(kv.Key))
                .Select(kv => kv.Value.Count)
                .Min();
            SortedSet<int> minIncludes = new SortedSet<int>(primeImplicants
                .Where(each => each.IncludeIds.Count == minIncludeCounts)
                .Select(each =>
                    each.IncludeIds
                    .Where(termId => !reducedTermId.Contains(termId))
                    .Select(termId => termId)
                )
                .SelectMany(each => each)
                .ToArray()
            );
            epiMin[epiMin.Count - 1] = minIncludes;
            epiMinCountIs1[epiMinCountIs1.Count - 1] = (minIncludeCounts == 1);

            // Mark Reducing Targets
            foreach (var kv in idIncludes)
            {
                if (kv.Value.Count == minIncludeCounts)
                {
                    reducedTermId.Add(kv.Key);
                    foreach(int listIdx in kv.Value)
                    {
                        if (!reducedListIdx.Contains(listIdx)) {
                            epiListIdxSets.Last().Add(listIdx);
                            reducedListIdx.Add(listIdx);
                        }
                    }
                }
            }
            foreach (var listIdx in reducedListIdx)
            {
                QMCTerm each = primeImplicants[listIdx];
                foreach (int termId in each.IncludeIds)
                {
                    reducedTermId.Add(termId);
                }
            }
        }
        
        Queue<SortedSet<QMCTerm>> queue = new Queue<SortedSet<QMCTerm>>(
            epiListIdxSets[0]
            .Select(listIdx => new SortedSet<QMCTerm>(new QMCTerm[] { primeImplicants[listIdx] }))
            .ToArray()
        );
        
        for (int i = 1; i < epiListIdxSets.Count; i++)
        {
            if (epiMin[i].Count == 1)
            {
                // Find to add
                SortedSet<QMCTerm> adding = new SortedSet<QMCTerm>();
                foreach (int listIdx in epiListIdxSets[i])
                {
                    adding.Add(primeImplicants[listIdx]);
                }
                // Add
                for (int q = 0; q < queue.Count; q++)
                {
                    SortedSet<QMCTerm> now = queue.Dequeue();
                    now.UnionWith(adding);
                    queue.Enqueue(now);
                }
            }
            else
            {
                // Find to add
                List<int> epiListIdxSet = epiListIdxSets[i].ToList();
                Queue<SortedSet<int>> termBuf = new Queue<SortedSet<int>>
                (
                    new SortedSet<int>[]
                    {
                        new SortedSet<int>(new int[]{ epiListIdxSet[0] })
                    }
                );
                for (int j = 1; j < epiListIdxSet.Count; j++)
                {
                    int settedTermId = epiListIdxSet[j];
                    SortedSet<int> settedTermIncludes = primeImplicants[settedTermId].IncludeIds;
                    for (int k = 0; k < termBuf.Count; k++)
                    {
                        SortedSet<int> buffedTermIds = termBuf.Dequeue();
                        SortedSet<int> buffedTermIncludes = new SortedSet<int>
                        (
                            buffedTermIds
                                .Select(termId => primeImplicants[termId])
                                .SelectMany(term => term.IncludeIds)
                        );
                        IEnumerable<int> settedTermExcepted = settedTermIncludes
                            .Except(buffedTermIncludes);
                        if (settedTermExcepted.Count() > 0)
                        {
                            buffedTermIds.Add(settedTermId);
                        }
                        termBuf.Enqueue(buffedTermIds);
                    }
                }
                int minCost = (int)1e9;
                for (int j = 0; j < termBuf.Count; j++)
                {
                    SortedSet<int> now = termBuf.Dequeue();
                    Math.Min(minCost, now.Count);
                    termBuf.Enqueue(now);
                }
                
                // Add
                for (int q = 0; q < queue.Count; q++)
                {
                    SortedSet<QMCTerm> now = queue.Dequeue();
                    for (int p = 0; p < termBuf.Count; p++)
                    {
                        SortedSet<int> buffedTermIds = termBuf.Dequeue();
                        if (buffedTermIds.Count > minCost) continue;
                        SortedSet<QMCTerm> buffedTerms = new SortedSet<QMCTerm>
                        (
                            buffedTermIds
                                .Select(termId => primeImplicants[termId])
                        );
                        now.UnionWith(buffedTerms);
                        termBuf.Enqueue(buffedTermIds);
                    }
                    queue.Enqueue(now);
                }
            }
        }

        return new List<SortedSet<QMCTerm>>(queue);
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
