namespace LoxLang.Core;

public partial class Parser
{
    private Expr ParseExpression()
        => ParseAssignment();

    private Expr ParseAssignment()
    {
        var expr = ParseLogicalOr();
        if (MatchToken(TokenType.EQUAL))
        {
            var token = Previous();
            var value = ParseAssignment();
            if (expr is VariableExpr { Name: var name })
                return new AssignExpr(name, value);
            GenerateError(token, "Invalid assignment target.");
        }
        return expr;
    }

    private Expr ParseLogicalOr()
        => ParseLeftAssociativeBinaryExpr(ParseLogicalAnd, static (left, op, right) => new LogicalExpr(left, op, right), TokenType.OR);

    private Expr ParseLogicalAnd()
        => ParseLeftAssociativeBinaryExpr(ParseEquality, static (left, op, right) => new LogicalExpr(left, op, right), TokenType.AND);

    private Expr ParseEquality()
        => ParseLeftAssociativeBinaryExpr(ParseComparison, static (left, op, right) => new BinaryExpr(left, op, right), TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL);

    private Expr ParseComparison()
        => ParseLeftAssociativeBinaryExpr(ParseTerm, static (left, op, right) => new BinaryExpr(left, op, right), TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL);

    private Expr ParseTerm()
        => ParseLeftAssociativeBinaryExpr(ParseFactor, static (left, op, right) => new BinaryExpr(left, op, right), TokenType.MINUS, TokenType.PLUS);

    private Expr ParseFactor()
        => ParseLeftAssociativeBinaryExpr(ParseUnary, static (left, op, right) => new BinaryExpr(left, op, right), TokenType.SLASH, TokenType.STAR);

    private Expr ParseUnary()
    {
        if (MatchToken(TokenType.BANG) || MatchToken(TokenType.MINUS))
            return new UnaryExpr(Previous(), ParseUnary());
        return ParseCall();
    }

    private Expr ParseCall()
    {
        var expr = ParsePrimary();
        while (true)
        {
            if (MatchToken(TokenType.LEFT_PAREN))
                expr = FinishCall(expr);
            if (MatchToken(TokenType.DOT))
                expr = new GetExpr(expr, Consume(TokenType.IDENTIFIER, "Expect property name after '.'."));
            else
                break;
        }
        return expr;

        Expr FinishCall(Expr expr)
        {
            var args = new List<Expr>();
            if (!Check(TokenType.RIGHT_PAREN))
                do
                {
                    if (args.Count >= 255)
                        GenerateError(Peek(), "Can't have more than 255 arguments.");
                    args.Add(ParseExpression());
                } while (MatchToken(TokenType.COMMA));
            var paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");
            return new CallExpr(expr, paren, args);
        }
    }

    private Expr ParsePrimary()
    {
        if (MatchToken(TokenType.FALSE))
            return new LiteralExpr(false);
        if (MatchToken(TokenType.TRUE))
            return new LiteralExpr(true);
        if (MatchToken(TokenType.NIL))
            return new LiteralExpr(null);
        if (MatchToken(TokenType.NUMBER) || MatchToken(TokenType.STRING))
            return new LiteralExpr(((LiteralToken)Previous()).Literal);
        if (MatchToken(TokenType.LEFT_PAREN))
        {
            var expr = ParseExpression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression.");
            return new GroupExpr(expr);
        }
        if (MatchToken(TokenType.IDENTIFIER))
            return new VariableExpr(Previous());
        if (MatchToken(TokenType.FUN))
            return ParseLambdaFunctionExpr();
        throw GenerateError(Peek(), "Expect expression.");
    }

    private Expr ParseLambdaFunctionExpr()
    {
        var paren = Consume(TokenType.LEFT_PAREN, $"Expect '(' after function name.");
        var parameters = new List<Token>();
        if (!Check(TokenType.RIGHT_PAREN))
            do
            {
                if (parameters.Count >= 255)
                    GenerateError(Peek(), "Can't have more than 255 parameters.");
                parameters.Add(Consume(TokenType.IDENTIFIER, "Expect parameter name."));
            } while (MatchToken(TokenType.COMMA));
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after parameters.");

        if (MatchToken(TokenType.LEFT_BRACE))
        {
            var body = ParseBlockStatement();

            return new LambdaExpr(parameters, body);
        }
        else if (MatchToken(TokenType.COLON))
        {
            var expr = ParseFuncStatement();

            return new LambdaExpr(parameters, new() { expr });
        }
        else
            throw GenerateError(Peek(), "Expected '{{' or ':' after function");
    }

    private Expr ParseLeftAssociativeBinaryExpr(Func<Expr> parseExpr, Func<Expr, Token, Expr, Expr> ctor, params TokenType[] tokenTypes)
    {
        var left = parseExpr();
        while (tokenTypes.Any(MatchToken))
        {
            var op = Previous();
            var right = parseExpr();
            left = ctor(left, op, right);
        }
        return left;
    }
}
