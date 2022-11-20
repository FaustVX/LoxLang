namespace LoxLang.Core;

public sealed class Interpretor : IExprVisitor<object?>, IStmtVisitor<Void>
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

    object? IExprVisitor<object?>.Visit(BinaryExpr expr)
        => (expr.Operator.TokenType, expr.Left.Accept(this), expr.Right.Accept(this)) switch
        {
            (TokenType.MINUS, var left, var right)
                when CheckNumberOperand(left, expr.Operator, out var l) && CheckNumberOperand(right, expr.Operator, out var r)
                => l - r,
#if CHALLENGE_INTERPRET
            (TokenType.SLASH, var left, var right)
                when CheckNumberOperand(left, expr.Operator, out var l) && CheckNumberOperand(right, expr.Operator, out var r) && r is 0
                => throw new RuntimeException(expr.Operator, "Division by 0 occured."),
#endif
            (TokenType.SLASH, var left, var right)
                when CheckNumberOperand(left, expr.Operator, out var l) && CheckNumberOperand(right, expr.Operator, out var r)
                => l / r,
            (TokenType.STAR, var left, var right)
                when CheckNumberOperand(left, expr.Operator, out var l) && CheckNumberOperand(right, expr.Operator, out var r)
                => l * r,
            (TokenType.PLUS, var left, var right) => AddOperands(expr.Operator, left, right),
            (TokenType.GREATER, var left, var right)
                when CheckNumberOperand(left, expr.Operator, out var l) && CheckNumberOperand(right, expr.Operator, out var r)
                => l > r,
            (TokenType.GREATER_EQUAL, var left, var right)
                when CheckNumberOperand(left, expr.Operator, out var l) && CheckNumberOperand(right, expr.Operator, out var r)
                => l >= r,
            (TokenType.LESS, var left, var right)
                when CheckNumberOperand(left, expr.Operator, out var l) && CheckNumberOperand(right, expr.Operator, out var r)
                => l < r,
            (TokenType.LESS_EQUAL, var left, var right)
                when CheckNumberOperand(left, expr.Operator, out var l) && CheckNumberOperand(right, expr.Operator, out var r)
                => l <= r,
            (TokenType.EQUAL_EQUAL, var l, var r) => IsEquals(l, r),
            (TokenType.BANG_EQUAL, var l, var r) => !IsEquals(l, r),
            _ => null,
        };

    object? IExprVisitor<object?>.Visit(LogicalExpr expr)
        => (expr.Operator.TokenType, expr.Left.Accept(this)) switch
        {
            (TokenType.OR, var left) when IsTruthy(left) => left,
            (TokenType.AND, var left) when !IsTruthy(left) => left,
            _ => expr.Right.Accept(this),
        };

    object? IExprVisitor<object?>.Visit(GroupExpr expr)
        => expr.Expression.Accept(this);

    object? IExprVisitor<object?>.Visit(LiteralExpr expr)
        => expr.Value;

    object? IExprVisitor<object?>.Visit(UnaryExpr expr)
        => (expr.Operator.TokenType, expr.Expression.Accept(this)) switch
        {
            // (TokenType.BANG, bool b) => !b,
            (TokenType.BANG, var o) => !IsTruthy(o),
            (TokenType.MINUS, var value)
                when CheckNumberOperand(value, expr.Operator, out var v)
                => -v,
            _ => null,
        };

    object? IExprVisitor<object?>.Visit(VariableExpr expr)
        => LookupVariable(expr.Name, expr);

    object? IExprVisitor<object?>.Visit(AssignExpr expr)
    {
        var value = expr.Value.Accept(this);
        if (_locals.TryGetValue(expr, out var distance))
            CurrentEnv.AssignAt(distance, expr.Name, value);
        else
            RootEnv.Assign(expr.Name, value);
        return value;
    }

    object? IExprVisitor<object?>.Visit(CallExpr expr)
    {
        var callee = expr.Callee.Accept(this);
        var args = expr.Arguments.Select(arg => arg.Accept(this)).ToList();
        if (callee is ICallable function)
        {
            if (function.Arity != args.Count)
                throw new RuntimeException(expr.Paren, $"Expected {function.Arity} arguments but got {args.Count}.");
            return function.Call(this, args);
        }
        throw new RuntimeException(expr.Paren, "Can only call functions and classes.");
    }

    object? IExprVisitor<object?>.Visit(LambdaExpr expr)
        => new LambdaFunction(expr, CurrentEnv);

    Void IStmtVisitor<Void>.Visit(ExprStmt stmt)
    {
        stmt.Expr.Accept(this);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(PrintStmt stmt)
    {
        Console.WriteLine(Stringify(stmt.Expr.Accept(this)));
        return default;
    }

    Void IStmtVisitor<Void>.Visit(VariableStmt stmt)
    {
        var value = stmt.Initializer?.Accept(this);
        CurrentEnv.Define(stmt.Name, value);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(BlockStmt stmt)
    {
        ExecuteBlock(stmt.Statements, new(CurrentEnv));
        return default;
    }

    Void IStmtVisitor<Void>.Visit(IfStmt stmt)
    {
        if (IsTruthy(stmt.Condition.Accept(this)))
            stmt.Then.Accept(this);
        else
            stmt.Else?.Accept(this);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(WhileStmt stmt)
    {
        try
        {
            while (IsTruthy(stmt.Condition.Accept(this)))
                stmt.Body.Accept(this);
        }
        catch (BreakControlFlowException)
        {
            return default;
        }
        return default;
    }

    Void IStmtVisitor<Void>.Visit(FunctionStmt stmt)
    {
        CurrentEnv.DefineFun(new Function(stmt, CurrentEnv));
        return default;
    }

    Void IStmtVisitor<Void>.Visit(ReturnStmt stmt)
    {
        var value = stmt.Expr?.Accept(this);
        throw new ReturnControlFlowException(value);
    }

    Void IStmtVisitor<Void>.Visit(BreakStmt stmt)
    {
        throw new BreakControlFlowException();
    }

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

    private static bool CheckNumberOperand(object? obj, Token token, out double value)
    {
        if (obj is double d)
        {
            value = d;
            return true;
        }
        throw new RuntimeException(token, "Operand must be a number.");
    }

    private static object AddOperands(Token token, object? left, object? right)
        => (left, right) switch
        {
            (double l, double r) => l + r,
            (string l, string r) => l + r,
#if CHALLENGE_INTERPRET
            (string l, double r) => l + r,
            (double l, string r) => l + r,
            _ => throw new RuntimeException(token, "Operands must be either a number or a string."),
#else
            _ => throw new RuntimeException(token, "Operands must be either both number or both string."),
#endif
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
