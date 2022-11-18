namespace LoxLang.Core;

public class ASTPrinterRPN : IVisitor<string>
{
    public string Print(Expr expr)
        => expr.Accept(this);

    string IVisitor<string>.Visit(BinaryExpr expr)
        => $"{expr.Left.Accept(this)} {expr.Right.Accept(this)} {expr.Operator.Lexeme.ToString()}";

    string IVisitor<string>.Visit(GroupExpr expr)
        => $"{expr.Expression.Accept(this)}";

    string IVisitor<string>.Visit(LiteralExpr expr)
        => expr.Value is {} value ? value.ToString()! : "nil";

    string IVisitor<string>.Visit(UnaryExpr expr)
        => $"{expr.Expression.Accept(this)} {expr.Operator.Lexeme.ToString()}";
}
