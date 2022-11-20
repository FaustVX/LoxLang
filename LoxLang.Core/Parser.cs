namespace LoxLang.Core;

public partial class Parser
{
    private enum FunctionKind
    {
        Function,
        Method,
    }

    private readonly IReadOnlyList<Token> _tokens;
    private int _current = 0;

    private bool IsAtEnd
        => Peek().TokenType == TokenType.EOF;

    public Parser(IReadOnlyList<Token> tokens)
    {
        this._tokens = tokens;
    }

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();
        while (!IsAtEnd)
            statements.Add(ParseDeclaration());
        return statements;
    }

    private Token Consume(TokenType token, string message)
    {
        if (Check(token))
            return Advance();
        throw GenerateError(Peek(), message);
    }

    private ParserException GenerateError(Token token, string message)
    {
        Lox.Error(token, message);
        return new();
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
