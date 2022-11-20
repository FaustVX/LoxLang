namespace LoxLang.Core;

public sealed partial class Interpretor : IExprVisitor<object?>
{
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
