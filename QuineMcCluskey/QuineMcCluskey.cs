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
            this.Compute();
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

    public void FindEssentialPrimeImplicants()
    {
        List<QMCTerm> _minimumSOPs = new List<QMCTerm>(this.PrimeImplicants);
        Commons.DefaultDictionary<int, SortedSet<int>> idTermMatch = new Commons.DefaultDictionary<int, SortedSet<int>>();
        bool[] isChecked = Enumerable.Repeat(false, _minimumSOPs.Count).ToArray();
        SortedSet<QMCTerm> essentialPrimeImplicants = new SortedSet<QMCTerm>();

        // Find essential prime impllicants
        for (int i = 0; i < _minimumSOPs.Count; i++)
        {
            QMCTerm now = _minimumSOPs[i];
            foreach (int includedId in now.IncludeIds)
            {
                idTermMatch[includedId].Add(i);
            }
        }

        SortedSet<int> reducingTarget = new SortedSet<int>();
        foreach (KeyValuePair<int, SortedSet<int>> kv in idTermMatch)
        {
            if (kv.Value.Count() == 1)
            {
                int idx = kv.Value.First();
                QMCTerm selected = _minimumSOPs[idx];
                if (!isChecked[idx])
                {
                    foreach (int includedId in selected.IncludeIds)
                    {
                        reducingTarget.Add(includedId);
                    }
                }
                essentialPrimeImplicants.Add(selected);
                isChecked[idx] = true;
            }
        }
        Console.WriteLine($"Essential Prime Implicants {string.Join(", ", essentialPrimeImplicants)}");

        // Find other prime implicants
        List<SortedSet<QMCTerm>> otherPrimeImplicants = new List<SortedSet<QMCTerm>>();
        while (true)
        {
            idTermMatch = new Commons.DefaultDictionary<int, SortedSet<int>>();
            for (int i = 0; i < _minimumSOPs.Count; i++)
            {
                if (isChecked[i])
                {
                    continue;
                }
                QMCTerm now = _minimumSOPs[i];
                foreach (int includedId in now.IncludeIds)
                {
                    if (reducingTarget.Contains(includedId))
                    {
                        continue;
                    }
                    idTermMatch[includedId].Add(i);
                }
            }
            if (idTermMatch.Values.Count <= 0)
            {
                break;
            }

            otherPrimeImplicants.Add(new SortedSet<QMCTerm>());
            int minCounts = idTermMatch.Values.Select(each => each.Count).Min();
            foreach (KeyValuePair<int, SortedSet<int>> kv in idTermMatch)
            {
                if (kv.Value.Count() == minCounts)
                {
                    int idx = kv.Value.First();
                    QMCTerm selected = _minimumSOPs[idx];
                    if (!isChecked[idx])
                    {
                        foreach (int includedId in selected.IncludeIds)
                        {
                            reducingTarget.Add(includedId);
                        }
                    }
                    otherPrimeImplicants.Last().Add(selected);
                    isChecked[idx] = true;
                    Array.Exists(isChecked, each => !each); // 종료 조건
                }
            }
        }
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
