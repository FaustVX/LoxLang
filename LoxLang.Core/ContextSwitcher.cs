global using static LoxLang.Core.Utils.ContextSwitcher;

namespace LoxLang.Core.Utils;

public readonly ref struct ContextSwitcher<T>
{
    private readonly ref T _ref;
    private readonly T _enclosing;

    public ContextSwitcher(ref T enclosing, T newContext)
    {
        _ref = ref enclosing;
        (_enclosing, _ref) = (_ref, newContext);
    }

    public void Dispose()
        => _ref = _enclosing;
}

public static class ContextSwitcher
{
    public static ContextSwitcher<T> Switch<T>(ref T enclosing, T newContext)
        => new(ref enclosing, newContext);
}
