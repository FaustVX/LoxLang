namespace LoxLang.Core;

public interface ICallable
{
    int Arity { get; }

    object? Call(Interpreter interpreter, List<object?> args);
}

public abstract class NativeCallable : ICallable
{
    public abstract int Arity { get; }
    public abstract string Name { get; }

    public abstract object? Call(Interpreter interpreter, List<object?> args);

    public override string ToString()
        => $"<native fn {Name}>";
}

public abstract class DefinedCallable : ICallable
{
    public abstract Token NameToken { get; }
    public abstract int Arity { get; }
    public virtual string Name
        => NameToken.Lexeme.ToString();

    public abstract object? Call(Interpreter interpreter, List<object?> args);

    public override string ToString()
        => $"<fn {Name}>";
}

public abstract class AnonymousCallable : ICallable
{
    public abstract int Arity { get; }

    public abstract object? Call(Interpreter interpreter, List<object?> args);

    public override string ToString()
        => $"<fn lambda>";
}

public sealed class Function : DefinedCallable
{
    private readonly FunctionStmt _stmt;
    private readonly Environment _closure;
    public Function(FunctionStmt stmt, Environment closure)
        => (_stmt, _closure) = (stmt, closure);

    public override int Arity
        => _stmt.Parameters.Count;

    public override Token NameToken
        => _stmt.Name;

    public override object? Call(Interpreter interpreter, List<object?> args)
    {
        var env = new Environment(_closure);
        for (int i = 0; i < Arity; i++)
            env.Define(_stmt.Parameters[i], args[i]);
        try
        {
            interpreter.ExecuteBlock(_stmt.Body, env);
        }
        catch (ReturnControlFlowException ex)
        {
            return ex.Value;
        }
        return null;
    }
}

public sealed class LambdaFunction : AnonymousCallable
{
    private readonly LambdaExpr _expr;
    private readonly Environment _closure;
    public LambdaFunction(LambdaExpr stmt, Environment closure)
        => (_expr, _closure) = (stmt, closure);

    public override int Arity
        => _expr.Parameters.Count;

    public override object? Call(Interpreter interpreter, List<object?> args)
    {
        var env = new Environment(_closure);
        for (int i = 0; i < Arity; i++)
            env.Define(_expr.Parameters[i], args[i]);
        try
        {
            interpreter.ExecuteBlock(_expr.Body, env);
        }
        catch (ReturnControlFlowException ex)
        {
            return ex.Value;
        }
        return null;
    }
}
