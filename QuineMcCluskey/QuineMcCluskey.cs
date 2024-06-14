using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QuineMcCluskey.Commons;
using QuineMcCluskey.Exceptions;
using QuineMcCluskey.Term;
namespace QuineMcCluskey;

public class QuineMcCluskey
{
    Term.QMCTerm term = new Term.QMCTerm();
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

    // this.MergeTerms()
    public List<List<QMCTerm>> MergeTerms() => this._MergeTerms();
    private List<List<QMCTerm>> _MergeTerms()
    {
        List<List<QMCTerm>> mergedTerms = new List<List<QMCTerm>>
        {
            new List<QMCTerm>
            (
                (
                    from id in this.TotalTerms
                    select this.Terms[Term.Term.HashIdSet(id)]
                ).ToList()
            )
        };

        int idx = 0;
        while (mergedTerms.Count > idx)
        {
            List<QMCTerm> now = mergedTerms[idx];
            SortedSet<string> idHashSets = new SortedSet<string>();
            for (int i = 0; i < now.Count; i++)
            {
                for (int j = i + 1; j < now.Count; j++)
                {
                    QMCTerm a = now[i], b = now[j], c = null;

                    if (a.Diff(b).diffCount == 1)
                    {
                        c = new QMCTerm(a.Merge(b));
                    }
                    if (c != null)
                    {
                        a.Deactivate();
                        b.Deactivate();
                        if (idHashSets.Contains(c.IdHash))
                        {
                            continue;
                        }
                        if (idx + 1 == mergedTerms.Count)
                        {
                            mergedTerms.Add(new List<QMCTerm>());
                        }
                        mergedTerms[idx + 1].Add(c);
                        idHashSets.Add(c.IdHash);
                    }
                }
            }
            idx++;
        }
        return mergedTerms;
    }

    // this.PrimeImplicants
    public SortedSet<QMCTerm> PrimeImplicants
    {
        get
        {
            SortedSet<QMCTerm> primeImplicants = new SortedSet<QMCTerm>();
            foreach (List<QMCTerm> eachList in this.MergeTerms())
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
    }

    // this.FindEssentialPrimeImplicants()
    // not work correctly.
    public List<SortedSet<QMCTerm>> FindEssentialPrimeImplicants() => this._FindEssentialPrimeImplicants();
    private List<SortedSet<QMCTerm>> _FindEssentialPrimeImplicants()
    {
        List<QMCTerm> primeImplicants = this.PrimeImplicants.ToList();
        List<SortedSet<int>> epiListIdxSets = new List<SortedSet<int>>();
        List<bool> epiMinCountIs1 = new List<bool>();
        List<SortedSet<int>> epiMin = new List<SortedSet<int>>();
        SortedSet<int> reducedTermId = new SortedSet<int>();
        SortedSet<int> reducedListIdx = new SortedSet<int>();
        SortedSet<int> allIds = new SortedSet<int>();
        foreach (QMCTerm each in primeImplicants)
        {
            foreach (int id in each.IncludeIds) 
            {
                allIds.Add(id);
            }
        }
        
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
        
        // Queue includes epis.
        // Now merge others by popping and pushing
        SortedSet<int> processedTerm = new SortedSet<int>();
        Queue<SortedSet<QMCTerm>> queue = new Queue<SortedSet<QMCTerm>>();
        foreach (int listIdx in epiListIdxSets[0])
        {
            QMCTerm now = primeImplicants[listIdx];
            queue.Enqueue(
                new SortedSet<QMCTerm>(new QMCTerm[] { now })
            );
            foreach (int id in now.IncludeIds)
            {
                processedTerm.Add(id);
            }
        }
        
        for (int i = 1; i < epiListIdxSets.Count; i++)
        {
            if (epiMin[i].Count == 1)
            {
                // Find to add
                // `adding` is term that be added to all of queue
                SortedSet<QMCTerm> adding = new SortedSet<QMCTerm>();
                foreach (int listIdx in epiListIdxSets[i])
                {
                    QMCTerm now = primeImplicants[listIdx];
                    adding.Add(now);
                    foreach (int id in now.IncludeIds)
                    {
                        processedTerm.Add(id);
                    }
                }
                // Add
                // apply `adding`
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
                SortedSet<int> targetRemainedTerms = new SortedSet<int>(allIds.Except(processedTerm));
                List<int> nowIdxSets = epiListIdxSets[i].ToList();
                Queue<Tuple<SortedSet<QMCTerm>, SortedSet<int>, int>> addingQueue = new Queue<Tuple<SortedSet<QMCTerm>, SortedSet<int>, int>>();

                // Push item will be queue's first element
                for (int j = 0; j < nowIdxSets.Count; j++)
                {
                    int nowIdx = nowIdxSets[j];
                    QMCTerm nowQMCTerm = primeImplicants[nowIdx];
                    SortedSet<int> includesTargetRemainedSet = new SortedSet<int>();
                    foreach (int id in nowQMCTerm.IncludeIds)
                    {
                        if (targetRemainedTerms.Contains(id))
                        {
                            includesTargetRemainedSet.Add(id);
                        }
                    }
                    addingQueue.Enqueue
                    (
                        new Tuple<SortedSet<QMCTerm>, SortedSet<int>, int>(new SortedSet<QMCTerm>(new QMCTerm[]{ nowQMCTerm }), includesTargetRemainedSet, includesTargetRemainedSet.Count)
                    );
                }

                // Make `adding` using Queue
                for (int _a = 0; _a < addingQueue.Count; _a++)
                {
                    var popped = addingQueue.Dequeue();
                    (SortedSet<QMCTerm> poppedQMCTerm, SortedSet<int> poppedIdSet, int cost) = popped;
                    
                    for (int j = 1; j < nowIdxSets.Count; j++)
                    {
                        int nowIdx = nowIdxSets[j];
                        QMCTerm nowQMCTerm = primeImplicants[nowIdx];
                        IEnumerable<int> nowIdSet = nowQMCTerm.IncludeIds.Intersect(targetRemainedTerms);
                        SortedSet<int> nowIdSetExcept = new SortedSet<int>(nowIdSet.Except(poppedIdSet));
                        // Update if exists changes to update
                        if (nowIdSetExcept.Count != 0)
                        {
                            SortedSet<QMCTerm> termSet = new SortedSet<QMCTerm>
                            (
                                poppedQMCTerm.Union(new QMCTerm[]{nowQMCTerm})
                            );
                            SortedSet<int> idSet = new SortedSet<int>(poppedIdSet.Union(nowIdSetExcept));
                            addingQueue.Enqueue(new Tuple<SortedSet<QMCTerm>, SortedSet<int>, int>(termSet, idSet, cost + nowIdSetExcept.Count));
                        }
                    }

                }

                // Find minCost
                int minCost = (int)1e9;
                for (int _a = 0; _a < addingQueue.Count; _a++)
                {
                    var popped = addingQueue.Dequeue();
                    int cost = popped.Item3;
                    minCost = Math.Min(minCost, cost);
                    addingQueue.Enqueue(popped);
                }

                for (int _a = 0; _a < addingQueue.Count; _a++)
                {
                    var now = addingQueue.Dequeue();
                    addingQueue.Enqueue(now);
                }

                // Add
                // apply `adding`
                for (int _q = 0; _q < queue.Count; _q++)
                {
                    SortedSet<QMCTerm> now = queue.Dequeue();
                    for (int _a = 0; _a < addingQueue.Count; _a++)
                    {
                        (SortedSet<QMCTerm> addingTerms, SortedSet<int> addingTermIds, int addingTermCosts) = addingQueue.Dequeue();
                        if (addingTermCosts == minCost)
                        {
                            SortedSet<QMCTerm> newSet = new SortedSet<QMCTerm>(now.Union(addingTerms));
                            queue.Enqueue(newSet);
                        }
                    }
                }

            }

        }

        return new List<SortedSet<QMCTerm>>(queue);
    }

    public List<string> RenderMinSOP()
    {
        List<QMCTerm> primeImplicants = this.PrimeImplicants.ToList();
        List<SortedSet<QMCTerm>> essentialPrimeImplicants = this.FindEssentialPrimeImplicants();
        Queue<string> stringBuffer = new Queue<string>();
        foreach(SortedSet<QMCTerm> termSet in essentialPrimeImplicants)
        {
            stringBuffer.Enqueue(
                string.Join(" + ",
                    termSet
                        .Select(term => term.ToVariables(this.Literals))
                )
            );
        }
        
        return stringBuffer.ToList();
    }
}
