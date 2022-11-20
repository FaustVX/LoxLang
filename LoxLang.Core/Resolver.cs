namespace LoxLang.Core;

public sealed class Resolver : IExprVisitor<Void>, IStmtVisitor<Void>
{
    private enum FunctionType
    {
        None,
        Function,
    }

    private enum LoopType
    {
        None,
        Loop,
    }

    private readonly Interpretor _interpretor;
    private readonly Stack<Dictionary<string, bool>> _scopes = new();
    private FunctionType _currentFunction = FunctionType.None;
    private LoopType _currentLoop = LoopType.None;

    public Resolver(Interpretor interpretor)
        => _interpretor = interpretor;

    Void IExprVisitor<Void>.Visit(BinaryExpr expr)
    {
        expr.Left.Accept(this);
        expr.Right.Accept(this);
        return default;
    }

    Void IExprVisitor<Void>.Visit(LogicalExpr expr)
    {
        expr.Left.Accept(this);
        expr.Right.Accept(this);
        return default;
    }

    Void IExprVisitor<Void>.Visit(GroupExpr expr)
    {
        expr.Expression.Accept(this);
        return default;
    }

    Void IExprVisitor<Void>.Visit(LiteralExpr expr)
        => default;

    Void IExprVisitor<Void>.Visit(UnaryExpr expr)
    {
        expr.Expression.Accept(this);
        return default;
    }

    Void IExprVisitor<Void>.Visit(VariableExpr expr)
    {
        if (_scopes.TryPeek(out var scope) && scope.TryGetValue(expr.Name.Lexeme.ToString(), out var defined) && !defined)
            Lox.Error(expr.Name, "Can't read local variable in its own initializer.");
        ResolveLocal(expr, expr.Name);
        return default;
    }

    Void IExprVisitor<Void>.Visit(AssignExpr expr)
    {
        expr.Value.Accept(this);
        ResolveLocal(expr, expr.Name);
        return default;
    }

    Void IExprVisitor<Void>.Visit(CallExpr expr)
    {
        expr.Callee.Accept(this);
        foreach (var arg in expr.Arguments)
            arg.Accept(this);
        return default;
    }

    Void IExprVisitor<Void>.Visit(LambdaExpr expr)
    {
        ResolveFunction(expr, FunctionType.Function);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(ExprStmt stmt)
    {
        stmt.Expr.Accept(this);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(PrintStmt stmt)
    {
        stmt.Expr.Accept(this);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(VariableStmt stmt)
    {
        Declare(stmt.Name);
        stmt.Initializer?.Accept(this);
        Define(stmt.Name);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(BlockStmt stmt)
    {
        using (_ = CreateScope())
            Resolve(stmt.Statements);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(IfStmt stmt)
    {
        stmt.Condition.Accept(this);
        stmt.Then.Accept(this);
        stmt.Else?.Accept(this);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(WhileStmt stmt)
    {
        (var enclosint, _currentLoop) = (_currentLoop, LoopType.Loop);
        stmt.Condition.Accept(this);
        stmt.Body.Accept(this);
        _currentLoop = enclosint;
        return default;
    }

    Void IStmtVisitor<Void>.Visit(FunctionStmt stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.Function);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(ReturnStmt stmt)
    {
        if (_currentFunction is FunctionType.None)
            Lox.Error(stmt.keyword, "Can't return from top-level code.");
        stmt.Expr?.Accept(this);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(BreakStmt stmt)
    {
        if (_currentLoop is LoopType.None)
            Lox.Error(stmt.keyword, "Can't break out of a loop.");
        return default;
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (var stmt in statements)
            stmt.Accept(this);
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        var i = 0;
        foreach (var scope in _scopes)
        {
            if (scope.ContainsKey(name.Lexeme.ToString()))
            {
                _interpretor.Resolve(expr, i);
                return;
            }
            i++;
        }
    }

    private void ResolveFunction(FunctionStmt stmt, FunctionType type)
    {
        (var enclosing, _currentFunction) = (_currentFunction, type);
        using (_ = CreateScope())
        {
            foreach (var param in stmt.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(stmt.Body);
        }
        _currentFunction = enclosing;
    }

    private void ResolveFunction(LambdaExpr expr, FunctionType type)
    {
        (var enclosing, _currentFunction) = (_currentFunction, type);
        using (_ = CreateScope())
        {
            foreach (var param in expr.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(expr.Body);
        }
        _currentFunction = enclosing;
    }

    private void Declare(Token name)
    {
        if (_scopes.TryPeek(out var scope))
        {
            if (scope.ContainsKey(name.Lexeme.ToString()))
                Lox.Error(name, "Already a variable with this name in this scope.");
            scope[name.Lexeme.ToString()] = false;
        }
    }

    private void Define(Token name)
    {
        if (_scopes.TryPeek(out var scope))
            scope[name.Lexeme.ToString()] = true;
    }

    private IDisposable CreateScope()
        => new Scope(this);

    private readonly struct Scope : IDisposable
    {
        private readonly Resolver _resolver;

        public Scope(Resolver resolver)
        {
            _resolver = resolver;
            _resolver._scopes.Push(new());
        }

        void IDisposable.Dispose()
            => _resolver._scopes.Pop();
    }
}
