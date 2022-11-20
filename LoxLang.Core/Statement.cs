namespace LoxLang.Core;

public interface IStmtVisitor<T>
{
    T Visit(ExprStmt stmt);
    T Visit(PrintStmt stmt);
    T Visit(VariableStmt stmt);
    T Visit(BlockStmt stmt);
    T Visit(IfStmt stmt);
    T Visit(WhileStmt stmt);
    T Visit(FunctionStmt stmt);
    T Visit(ReturnStmt stmt);
    T Visit(BreakStmt stmt);
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

public sealed record class VariableStmt(Token Name, Expr Initializer) : Stmt()
{
    public override T Accept<T>(IStmtVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class BlockStmt(List<Stmt> Statements) : Stmt()
{
    public BlockStmt(params Stmt[] statements)
    : this(new List<Stmt>(statements))
    { }
    public override T Accept<T>(IStmtVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class IfStmt(Expr Condition, Stmt Then, Stmt? Else) : Stmt()
{
    public override T Accept<T>(IStmtVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class WhileStmt(Expr Condition, Stmt Body) : Stmt()
{
    public override T Accept<T>(IStmtVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class FunctionStmt(Token Name, List<Token> Parameters, List<Stmt> Body) : Stmt()
{
    public override T Accept<T>(IStmtVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class ReturnStmt(Token keyword, Expr? Expr) : Stmt()
{
    public override T Accept<T>(IStmtVisitor<T> visitor)
        => visitor.Visit(this);
}

public sealed record class BreakStmt(Token keyword) : Stmt()
{
    public override T Accept<T>(IStmtVisitor<T> visitor)
        => visitor.Visit(this);
}
