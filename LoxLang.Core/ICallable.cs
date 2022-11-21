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
    private readonly bool _isInit;

    public Function(FunctionStmt stmt, Environment closure, bool isInit)
        => (_stmt, _closure, _isInit) = (stmt, closure, isInit);

    public override int Arity
        => _stmt.Parameters.Count;

    public override Token NameToken
        => _stmt.Name;
    public bool IsGetter => _stmt is GetterStmt;

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
            if (_isInit)
                return _closure.GetAt(0, "init");
            return ex.Value;
        }
        if (_isInit)
            return _closure.GetAt(0, "this");
        return null;
    }

    public Function Bind(Instance instance)
    {
        var env = new Environment(_closure);
        env.DefineFun(instance);
        return new Function(_stmt, env, _isInit);
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

public sealed class Class : DefinedCallable
{
    public override Token NameToken { get; }
    private readonly Dictionary<string, Function> _methods;

    public Class(Token name, Dictionary<string, Function> methods)
        => (NameToken, _methods) = (name, methods);

    public override int Arity => FindMethod("init")?.Arity ?? 0;

    public override object? Call(Interpreter interpreter, List<object?> args)
    {
        var instance = new Instance(this);
        FindMethod("init")?.Bind(instance).Call(interpreter, args);
        return instance;
    }

    public override string ToString()
        => $"<class {Name}>";

    public Function? FindMethod(string name)
        => _methods.TryGetValue(name, out var method) ? method : null;
}

public class Instance
{
    private readonly Class _class;
    private readonly Dictionary<string, object?> _fileds;

    public Instance(Class @class)
    {
        _class = @class;
        _fileds = new()
        {
            ["className"] = _class.Name,
        };
    }

    public override string ToString()
        => $"<{_class.Name}>";

    public object? Get(Token name)
        => _fileds.TryGetValue(name.Lexeme.ToString(), out var value) ? value
        :  _class.FindMethod(name.Lexeme.ToString()) is {} method ? method.Bind(this)
        :  throw new RuntimeException(name, $"Undefined property '{name.Lexeme}'.");

    public object? Set(Token name, object? value)
        => _fileds[name.Lexeme.ToString()] = value;
}
