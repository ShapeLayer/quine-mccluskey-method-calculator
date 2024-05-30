using System.Collections;
using System.Collections.Generic;
namespace QuineMcCluskey.Commons;

public enum Bit
{
    F = 0,
    T = 1,
    X = -1
};

public class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : new()
{
    public new TValue this[TKey key]
    {
        get
        {
            TValue val;
            if (!TryGetValue(key, out val))
            {
                val = new TValue();
                Add(key, val);
            }
            return val;
        }
        set { base[key] = value; }
    }
}
