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
        => QuineMcCluskey.GetPrimeImplicants(this.terms);

    public void Compute() => this.FindPrimeImplicants();

    public void FindPrimeImplicants()
    {
        List<List<QMCTerm>> stages = QuineMcCluskey.MergeTerms(this.terms[0]);
        this.terms.Clear();
        this.terms.AddRange(stages);
    }

    public List<string> RenderMinSOP()
        => QuineMcCluskey.RenderMinSOP(this.FindEssentialPrimeImplicants(), this.variableExpressions);

    public List<SortedSet<QMCTerm>> FindEssentialPrimeImplicants()
    {
        this.FindPrimeImplicants();
        return this._FindEssentialPrimeImplicants();
    }

    public List<SortedSet<QMCTerm>> _FindEssentialPrimeImplicants()
        => QuineMcCluskey.FindMinimumCovers(
            this.GetPrimeImplicants(), this.terms[0].SelectMany(term => term.IncludeIds));

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
