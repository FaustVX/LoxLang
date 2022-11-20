namespace LoxLang.Core;

public sealed partial class Interpretor
{
    private sealed class ClockFun : NativeCallable
    {
        public override int Arity => 0;
        public override string Name => "clock";

        public override object? Call(Interpretor interpretor, List<object?> args)
            => DateTime.UtcNow.TimeOfDay.TotalSeconds;
    }

    private sealed class ReadFun : NativeCallable
    {
        public override int Arity => 0;
        public override string Name => "read";

        public override object? Call(Interpretor interpretor, List<object?> args)
            => Console.ReadLine();
    }

    private sealed class PrintFun : NativeCallable
    {
        public override int Arity => 1;
        public override string Name => "print";

        public override object? Call(Interpretor interpretor, List<object?> args)
        {
            Console.WriteLine(args[0]);
            return default(Void);
        }
    }

    private sealed class BreakpointFun : NativeCallable
    {
        public override int Arity => 0;
        public override string Name => "breakpoint";

        public override object? Call(Interpretor interpretor, List<object?> args)
        {
           Lox.LaunchDebugger();
            return default(Void);
        }
    }

    private static readonly Environment _globalEnv = new();
    public Environment RootEnv { get; }
    public Environment CurrentEnv { get; private set; }
    private readonly Dictionary<Expr, int> _locals = new();

    static Interpretor()
    {
        _globalEnv.DefineFun(new ClockFun());
        _globalEnv.DefineFun(new ReadFun());
        _globalEnv.DefineFun(new PrintFun());
        _globalEnv.DefineFun(new BreakpointFun());
    }

    public Interpretor()
    {
        RootEnv = new(_globalEnv);
        CurrentEnv = RootEnv;
    }

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var stmt in statements)
                stmt.Accept(this);
        }
        catch (RuntimeException ex)
        {
            Lox.RuntimeError(ex);
        }
    }

    private static string Stringify(object? value)
        => value switch
        {
            null => "nil",
            var v => v.ToString()!,
        };

    public void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        var previous = CurrentEnv;
        try
        {
            CurrentEnv = environment;
            foreach (var stmt in statements)
                stmt.Accept(this);
        }
        finally
        {
            CurrentEnv = previous;
        }
    }

    private static bool IsTruthy(object? obj)
        => obj switch
        {
            null => false,
            bool b => b,
            _ => true,
        };

    private static bool IsEquals(object? left, object? right)
        => (left, right) switch
        {
            (null, null) => true,
            (null, _) or (_, null) => false,
            (var l, var r) => l.Equals(r),
        };

    public void Resolve(Expr expr, int depth)
    {
        _locals[expr] = depth;
    }

    private object? LookupVariable(Token name, Expr expr)
    {
        if (_locals.TryGetValue(expr, out var distance))
            return CurrentEnv.GetAt(distance, name.Lexeme.ToString());
        return RootEnv.Get(name);
    }
}
