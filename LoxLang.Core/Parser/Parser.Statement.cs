namespace LoxLang.Core;

public partial class Parser
{
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
        if (MatchToken(TokenType.BREAK))
        {
            var token = Previous();
            Consume(TokenType.SEMICOLON, "Expected ';' after 'break'.");
            return new BreakStmt(token);
        }
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
}
