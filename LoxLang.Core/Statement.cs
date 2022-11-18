namespace LoxLang.Core;

public interface IStmtVisitor<T>
{
    T Visit(ExprStmt stmt);
    T Visit(PrintStmt stmt);
    T Visit(VariableStmt stmt);
    T Visit(BlockStmt stmt);
}

public abstract record class Stmt()
{
    public abstract T Accept<T>(IStmtVisitor<T> visitor);
}

public sealed record class ExprStmt(Expr Expr) : Stmt()
{
    public override T Accept<T>(IStmtVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class PrintStmt(Expr Expr) : Stmt()
{
    public override T Accept<T>(IStmtVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class VariableStmt(Token Name, Expr? Initializer) : Stmt()
{
    public override T Accept<T>(IStmtVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class BlockStmt(List<Stmt> Statements) : Stmt()
{
    public override T Accept<T>(IStmtVisitor<T> visitor)
        => visitor.Visit(this);
}
