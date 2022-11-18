namespace LoxLang.Core;

public interface IVisitor<T>
{
    T Visit(BinaryExpr expr);
    T Visit(GroupExpr expr);
    T Visit(LiteralExpr expr);
    T Visit(UnaryExpr expr);
}

public abstract record class Expr()
{
    public abstract T Accept<T>(IVisitor<T> visitor);
}
public sealed record class BinaryExpr(Expr Left, Token Operator, Expr Right) : Expr()
{
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class GroupExpr(Expr Expression) : Expr()
{
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class LiteralExpr(object? Value) : Expr()
{
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class UnaryExpr(Token Operator, Expr Expression) : Expr()
{
    public override T Accept<T>(IVisitor<T> visitor)
        => visitor.Visit(this);
}
