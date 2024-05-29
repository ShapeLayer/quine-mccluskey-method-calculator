using System;
namespace QuineMcCluskey.Exceptions;

[Serializable]
public class TermDiffCountNot1Error: System.Exception
{
    public TermDiffCountNot1Error() : base() { }
}
