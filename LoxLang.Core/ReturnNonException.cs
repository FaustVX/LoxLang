namespace LoxLang.Core;

[Serializable]
public class ReturnControlFlowException : Exception
{
    public object? Value { get; }

    public ReturnControlFlowException(object? value)
    {
        Value = value;
    }
}

[Serializable]
public class BreakControlFlowException : Exception
{ }
