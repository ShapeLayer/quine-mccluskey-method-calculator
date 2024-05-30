using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using QuineMcCluskey.Commons;
using QuineMcCluskey.Exceptions;
namespace QuineMcCluskey.Term;

public class Term : IComparable
{
    int _size;
    Bit[] _bits;
    SortedSet<int> _includeIds;

    public int size { get { return _size; } }
    public Bit[] bits { get { return _bits; } }
    public SortedSet<int> IncludeIds { get { return _includeIds; } }

    public Term()
    {
        this._size = 0;
        this._bits = new Bit[0];
    }
    protected Term(int size, Bit[] bits)
    {
        this._size = size;
        this._bits = bits;
    }
    public Term(int size, Bit[] bits, int id) : this(size, bits)
    {
        this._includeIds = new SortedSet<int>(new int[]{ id });
    }
    public Term(int size, Bit[] bits, int[] includeIds) : this(size, bits)
    {
        this._includeIds = new SortedSet<int>(includeIds);
    }
    public Term(int size, Bit[] bits, SortedSet<int> includeIds) : this(size, bits)
    {
        this._includeIds = includeIds;
    }
    public Term(int size, int id)
    {
        string binary = Convert.ToString(id, 2).PadLeft(size, '0');
        this._size = size;
        this._bits = new Bit[size];
        for (int i = 0; i < size; i++)
        {
            this._bits[i] = binary[i] == '0' ? Bit.F : Bit.T;
        }
        this._includeIds = new SortedSet<int>() { id };
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        Term other = obj as Term;
        if (this.IncludeIds.Count != other.IncludeIds.Count)
        {
            return this.IncludeIds.Count - other.IncludeIds.Count;
        }
        return this.IncludeIds.First() - other.IncludeIds.First();
    }

    public TermDiff Diff(Term other)
    {
        int diffCount = 0;
        bool[] diffs = new bool[this.size];
        for (int i = 0; i < this.size; i++)
        {
            diffs[i] = bits[i] != other.bits[i];
            if (diffs[i])
            {
                diffCount++;
            }
        }
        return new TermDiff(diffCount, diffs);
    }
    public Term Merge(Term other)
    {
        return Merge(other, Diff(other));
    }
    public Term Merge(Term other, TermDiff diff)
    {
        if (diff.diffCount != 1)
        {
            throw new TermDiffCountNot1Error();
        }
        // Merge Bits
        Bit[] newBits = new Bit[this.size];
        for (int i = 0; i < this.size; i++) { newBits[i] = diff.diffs[i] ? Bit.X : this.bits[i]; }
        // Merge ID
        SortedSet<int> newIds = new SortedSet<int>();
        newIds.UnionWith(this.IncludeIds);
        newIds.UnionWith(other.IncludeIds);
        return new Term(this.size, newBits, newIds);
    }

    public override string ToString()
    {
        List<string> eachBits = new List<string>();
        foreach (Bit bit in bits)
        {
            switch (bit)
            {
                case Bit.F:
                    eachBits.Add("0");
                    break;
                case Bit.T:
                    eachBits.Add("1");
                    break;
                case Bit.X:
                    eachBits.Add("-");
                    break;
            }
        }
        // return $"{eachBits.Count} {includeIds.Count}";
        return $"{string.Join("", eachBits)} ({string.Join(", ", IncludeIds)})";
    }

    public string ToVariables(string variables)
    {
        if (this.size != variables.Length)
            throw new BitLenVarCountNotMatchError();

        string[] rendered = new string[this.size];
        for (int i = 0; i < this.size; i++)
        {
            switch (this.bits[i])
            {
                case Bit.X:
                    rendered[i] = "";
                    break;
                case Bit.F:
                    rendered[i] = $"{variables[i]}'";
                    break;
                case Bit.T:
                    rendered[i] = $"{variables[i]}";
                    break;
            }
        }
        return string.Join("", rendered);
    }
}

public record TermDiff {
    public required int diffCount { get; init; }
    public required bool[] diffs { get; init; }
    
    [SetsRequiredMembers]
    public TermDiff(int diffCount, bool[] diffs)
    {
        this.diffCount = diffCount;
        this.diffs = diffs;
    }
}
