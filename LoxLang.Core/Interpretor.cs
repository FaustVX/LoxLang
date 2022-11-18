namespace LoxLang.Core;

public sealed class Interpretor : IVisitor<object?>
{
    public void Interpret(Expr exprpression)
    {
        try
        {
            var value = exprpression.Accept(this);
            Console.WriteLine(Stringify(value));
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

    object? IVisitor<object?>.Visit(BinaryExpr expr)
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

    object? IVisitor<object?>.Visit(GroupExpr expr)
        => expr.Expression.Accept(this);

    object? IVisitor<object?>.Visit(LiteralExpr expr)
        => expr.Value;

    object? IVisitor<object?>.Visit(UnaryExpr expr)
        => (expr.Operator.TokenType, expr.Expression.Accept(this)) switch
        {
            // (TokenType.BANG, bool b) => !b,
            (TokenType.BANG, var o) => !IsTruthy(o),
            (TokenType.MINUS, var value)
                when CheckNumberOperand(value, expr.Operator, out var v)
                => -v,
            _ => null,
        };

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
}
