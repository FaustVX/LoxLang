namespace LoxLang.Core;

public interface IExprVisitor<T>
{
    T Visit(BinaryExpr expr);
    T Visit(LogicalExpr expr);
    T Visit(GroupExpr expr);
    T Visit(LiteralExpr expr);
    T Visit(UnaryExpr expr);
    T Visit(VariableExpr expr);
    T Visit(AssignExpr expr);
}

public abstract record class Expr()
{
    public abstract T Accept<T>(IExprVisitor<T> visitor);
}
public sealed record class BinaryExpr(Expr Left, Token Operator, Expr Right) : Expr()
{
    public override T Accept<T>(IExprVisitor<T> visitor)
        => visitor.Visit(this);
}
public sealed record class LogicalExpr(Expr Left, Token Operator, Expr Right) : Expr()
{
    public override T Accept<T>(IExprVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class GroupExpr(Expr Expression) : Expr()
{
    public override T Accept<T>(IExprVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class LiteralExpr(object? Value) : Expr()
{
    public override T Accept<T>(IExprVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class UnaryExpr(Token Operator, Expr Expression) : Expr()
{
    public override T Accept<T>(IExprVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class VariableExpr(Token Name) : Expr()
{
    public override T Accept<T>(IExprVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class AssignExpr(Token Name, Expr Value) : Expr()
{
    public override T Accept<T>(IExprVisitor<T> visitor)
        => visitor.Visit(this);
}
