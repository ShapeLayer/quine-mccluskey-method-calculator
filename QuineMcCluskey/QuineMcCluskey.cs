using System.Collections;
using System.Collections.Generic;
using QuineMcCluskey.Commons;
using QuineMcCluskey.Exceptions;
using QuineMcCluskey.Term;
namespace QuineMcCluskey;

public class QuineMcCluskey
{
    
}

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

    public void _Computing()
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
                    try
                    {
                        c = (QMCTerm)a.Merge(b);
                    }
                    catch (Exception e) when (e is TermDiffCountNot1Error) {}
                    if (c != null)
                    {
                        if (idx + 1 == terms.Count)
                        {
                            this.terms.Add(new List<QMCTerm>());
                        }
                        this.terms[idx].Add(c);
                        a.Deactivate();
                        b.Deactivate();
                    }
                }
            }
        }
    }
}


public class QMCTerm: Term.Term
{
    bool _isActivated = true;
    public bool isActivated { get { return _isActivated; } }
    public void Activate() { this._isActivated = true; }
    public void Deactivate() { this._isActivated = false; }

    protected QMCTerm(int size, Bit[] bits): base(size, bits) {}
    public QMCTerm(int size, Bit[] bits, int id): base(size, bits, id) {}
    public QMCTerm(int size, Bit[] bits, int[] includeIds): base(size, bits, includeIds) {}
}

