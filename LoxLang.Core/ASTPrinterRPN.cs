namespace LoxLang.Core;

public class ASTPrinterRPN : IVisitor<string>
{
    public string Print(Expr expr)
        => expr.Accept(this);

    public string Visit(BinaryExpr expr)
        => $"{expr.Left.Accept(this)} {expr.Right.Accept(this)} {expr.Operator.Lexeme.ToString()}";

    public string Visit(GroupExpr expr)
        => $"({expr.Expression.Accept(this)})";

    public string Visit(LiteralExpr expr)
        => expr.Value is {} value ? value.ToString()! : "nil";

    public string Visit(UnaryExpr expr)
        => $"{expr.Expression.Accept(this)} {expr.Operator.Lexeme.ToString()}";
}
