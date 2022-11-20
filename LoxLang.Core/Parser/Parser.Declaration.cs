namespace LoxLang.Core;

partial class Parser
{
    private Stmt ParseDeclaration()
    {
        try
        {
            if (MatchToken(TokenType.FUN))
                return ParseFunction(FunctionKind.Function);
            if (MatchToken(TokenType.VAR))
                return ParseVarDeclaration();
            if (MatchToken(TokenType.CLASS))
                return ParseClassDeclaration();
            return ParseStatement();
        }
        catch (ParserException)
        {
            Synchronize();
            return null!;
        }
    }

    private Stmt ParseClassDeclaration()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect class name.");
        Consume(TokenType.LEFT_BRACE, "Expect '{' before class body.");
        var methods = new List<FunctionStmt>();
        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd)
            methods.Add(ParseFunction(FunctionKind.Method));
        Consume(TokenType.RIGHT_BRACE, "Expect '}' after class body.");
        return new ClassStmt(name, methods);
    }

    private FunctionStmt ParseFunction(FunctionKind funKind)
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
            var token = Previous();
            var expr = (ExprStmt)ParseExprStatement();

            return new FunctionStmt(name, parameters, new() { new ReturnStmt(token, expr.Expr) });
        }
        else
            throw GenerateError(Peek(), "Expected '{{' or ':' after function");
    }

    private Stmt ParseVarDeclaration()
    {
        var name = Consume(TokenType.IDENTIFIER, "Expect variable name.");
        Consume(TokenType.EQUAL, $"Variable '{name.Lexeme}' must be initialized.");
        var initializer = ParseExpression();
        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        return new VariableStmt(name, initializer);
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
}
