namespace LoxLang.Core;

public sealed partial class Resolver : IExprVisitor<Void>
{
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
        if (_scopes.TryPeek(out var scope) && scope.TryGetValue(expr.Name.Lexeme.ToString(), out var infos) && !infos.defined)
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
            ReportVariableUsage();
        }
        _currentFunction = enclosing;
    }
}
