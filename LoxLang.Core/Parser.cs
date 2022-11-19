namespace LoxLang.Core;

internal class Parser
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

    private Stmt ParseDeclaration()
    {
        try
        {
            if (MatchToken(TokenType.FUN))
                return ParseFunction(FunctionKind.Function);
            if (MatchToken(TokenType.VAR))
                return ParseVarDeclaration();
            return ParseStatement();
        }
        catch (ParserException)
        {
            Synchronize();
            return null!;
        }
    }

    private Stmt ParseFunction(FunctionKind funKind)
    {
        var kind = funKind.ToString().ToLower();
        var name = Consume(TokenType.IDENTIFIER, $"Expect {kind} name.");

        var paren = Consume(TokenType.LEFT_PAREN, $"Expect '(' after {kind} name.");
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

            return new FunctionStmt(name, parameters, body);
        }
        else if (MatchToken(TokenType.COLON))
        {
            var expr = ParseFuncStatement();

            return new FunctionStmt(name, parameters, new() { expr });
        }
        else
            throw GenerateError(Peek(), "Expected '{{' or ':' after function");
    }

    private Stmt ParseVarDeclaration()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");
#if CHALLENGE_STATEMENT
        Consume(TokenType.EQUAL, $"Variable '{name.Lexeme}' must be initialized.");
        var initializer = ParseExpression();
#else
        var initializer = MatchToken(TokenType.EQUAL) ? ParseExpression() : null;
#endif
        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        return new VariableStmt(name, initializer);
    }

    private Stmt ParseStatement()
    {
        if (MatchToken(TokenType.LEFT_BRACE))
            return new BlockStmt(ParseBlockStatement());
        if (MatchToken(TokenType.IF))
            return ParseIf();
        if (MatchToken(TokenType.WHILE))
            return ParseWhile();
        if (MatchToken(TokenType.FOR))
            return ParseFor();
        return ParseFuncStatement();
    }

    private Stmt ParseFuncStatement()
    {
        if (MatchToken(TokenType.PRINT))
            return ParsePrintStatement();
        if (MatchToken(TokenType.RETURN))
            return ParseReturn();
        return ParseExprStatement();
    }

    private Stmt ParseReturn()
    {
        var keyword = Previous();
        var expr = !MatchToken(TokenType.SEMICOLON) ? ParseExpression() : null;
        Consume(TokenType.SEMICOLON, "Expect ';' after return.");
        return new ReturnStmt(keyword, expr);
    }

    private Stmt ParseFor()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
        var initializer = Initializer(this);
        var condition = !MatchToken(TokenType.SEMICOLON) ? ParseExpression() : null;
        Consume(TokenType.SEMICOLON, "Expect ';' after loop condition.");
        var increment = !MatchToken(TokenType.RIGHT_PAREN) ? ParseExpression() : null;
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after clauses.");
        var body = ParseStatement();

        if (increment is not null)
            body = new BlockStmt(body, new ExprStmt(increment));
        condition ??= new LiteralExpr(true);
        body = new WhileStmt(condition, body);
        if (initializer is not null)
            body = new BlockStmt(initializer, body);
        return body;

        static Stmt? Initializer(Parser parser)
        {
            if (parser.MatchToken(TokenType.SEMICOLON))
                return null;
            if (parser.MatchToken(TokenType.VAR))
                return parser.ParseVarDeclaration();
            return parser.ParseExprStatement();
        }

    }

    private Stmt ParseWhile()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
        var cond = ParseExpression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after while condition.");
        var thenBranch = ParseStatement();
        return new WhileStmt(cond, thenBranch);
    }

    private Stmt ParseIf()
    {
        Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
        var cond = ParseExpression();
        Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");
        var thenBranch = ParseStatement();
        var elseBranch = MatchToken(TokenType.ELSE) ? ParseStatement() : null;
        return new IfStmt(cond, thenBranch, elseBranch);
    }

    private List<Stmt> ParseBlockStatement()
    {
        var statements = new List<Stmt>();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd)
            statements.Add(ParseDeclaration());
        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }

    private Stmt ParsePrintStatement()
    {
        var expr = ParseExpression();
        Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        return new PrintStmt(expr);
    }

    private Stmt ParseExprStatement()
    {
        var expr = ParseExpression();
        Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        return new ExprStmt(expr);
    }

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
