namespace LoxLang.Core;

internal class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _current = 0;

    private bool IsAtEnd
        => Peek().TokenType == TokenType.EOF;

    public Parser(IReadOnlyList<Token> tokens)
    {
        this._tokens = tokens;
    }

    public Expr? Parse()
    {
        try
        {
            return ParseExpression();
        }
        catch (ParserException)
        {
            return null;
        }
    }

    private Expr ParseExpression()
        => ParseEquality();

    private Expr ParseEquality()
        => ParseLeftAssociativeBinaryExpr(ParseComparison, TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL);

    private Expr ParseComparison()
        => ParseLeftAssociativeBinaryExpr(ParseTerm, TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL);

    private Expr ParseTerm()
        => ParseLeftAssociativeBinaryExpr(ParseFactor, TokenType.MINUS, TokenType.PLUS);

    private Expr ParseFactor()
        => ParseLeftAssociativeBinaryExpr(ParseUnary, TokenType.SLASH, TokenType.STAR);

    private Expr ParseUnary()
    {
        if (MatchToken(TokenType.BANG) || MatchToken(TokenType.MINUS))
            return new UnaryExpr(Previous(), ParseUnary());
        return ParsePrimary();
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
        throw GenerateError(Peek(), "Expect expression.");
    }

    private Token Consume(TokenType token, string message)
    {
        if (Check(token))
            return Advance();
        throw GenerateError(Peek(), message);
    }

    private void Synchronize()
    {
        Advance();
        while (!IsAtEnd)
        {
            if (Previous().TokenType is TokenType.SEMICOLON)
                return;
            switch (Peek().TokenType)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }
            Advance();
        }
    }

    private ParserException GenerateError(Token token, string message)
    {
        Lox.Error(token, message);
        return new();
    }

    private Expr ParseLeftAssociativeBinaryExpr(Func<Expr> parseExpr, params TokenType[] tokenTypes)
    {
        var left = parseExpr();
        while (tokenTypes.Any(MatchToken))
        {
            var op = Previous();
            var right = parseExpr();
            left = new BinaryExpr(left, op, right);
        }
        return left;
    }

    private bool MatchToken(TokenType token)
    {
        if (Check(token))
        {
            Advance();
            return true;
        }
        return false;
    }

    private Token Advance()
    {
        if (!IsAtEnd)
            _current++;
        return Previous();
    }

    private bool Check(TokenType token)
    {
        if (IsAtEnd)
            return false;
        return Peek().TokenType == token;
    }

    private Token Peek()
        => _tokens[_current];

    private Token Previous()
        => _tokens[_current - 1];

    private class ParserException : Exception
    {
    }
}
