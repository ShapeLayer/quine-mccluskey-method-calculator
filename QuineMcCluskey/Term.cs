using System.Diagnostics.CodeAnalysis;
using QuineMcCluskey.Commons;
using QuineMcCluskey.Exceptions;
namespace QuineMcCluskey.Term;

public class Term
{
    int _size;
    Bit[] _bits;
    int[] _includeIds;

    public int size { get { return _size; } }
    public Bit[] bits { get { return _bits; } }
    public int[] includeIds { get { return _includeIds; } }

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
    public Term(int size, Bit[] bits, int id): this(size, bits)
    {
        this._includeIds = new int[]{ id };
    }
    public Term(int size, Bit[] bits, int[] includeIds): this(size, bits)
    {
        this._includeIds = includeIds;
    }

    public TermDiff Diff(Term other)
    {
        int diffCount = 0;
        bool[] diffs = new bool[size];
        for (int i = 0; i < size; i++)
        {
            diffs[i] = bits[i] != other.bits[i];
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
        int[] newIds = new int[this.includeIds.Length + other.includeIds.Length];
        for (int i = 0; i < this.includeIds.Length; i++) { newIds[i] = this.includeIds[i]; }
        for (int i = 0; i < other.includeIds.Length; i++) { newIds[this.includeIds.Length + i] = other.includeIds[i]; }
        return new Term(this.size, newBits, newIds);
    }
}

public record TermDiff {
    public required int diffCount { get; init; }
    public required bool[] diffs { get; init; }
    
    [SetsRequiredMembers]
    public TermDiff(int diffCount, bool[] diffs) => (diffCount, diffs) = (diffCount, diffs);
}
