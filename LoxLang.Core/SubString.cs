namespace LoxLang.Core;

public readonly struct SubString
{
    private SubString(SubString s, int start, int end)
    {
        _base = s._base;
        Start = start;
        Length = end - Start;
        if (End > s.End)
            throw new();
    }

    public SubString(ReadOnlyMemory<char> s)
    {
        _base = s;
        Start = 0;
        Length = _base.Length;
    }

    private readonly ReadOnlyMemory<char> _base;
    public readonly int Start;
    public readonly int Length;
    public readonly int End => Start + Length;

    public SubString this[Range range]
        => new(this, Start + range.Start.GetOffset(Length), Start + range.End.GetOffset(Length));
    public char this[Index index]
        => _base.Span[index.GetOffset(Length) + Start];

    public ReadOnlySpan<char> GetSpan()
        => _base.Span[Start..Length];

    public static implicit operator SubString(string s)
        => new(s.AsMemory());

    public static implicit operator SubString(ReadOnlyMemory<char> s)
        => new(s);

    public override string ToString()
        => new(_base.Span.Slice(Start, Length));
}
