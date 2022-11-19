namespace LoxLang.Core;

public static class Lox
{
    public static bool HasError { get; set; }
    public static bool HasRuntimeError { get; set; }
    private readonly static Interpretor _interpretor = new Interpretor();

    public static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        var stmts = parser.Parse();

        if (HasError)
            return;

        _interpretor.Interpret(stmts);
    }

    public static void Error(int line, string message)
        => Report(line, "", message);

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line: {line}] Error{where}: {message}");
        HasError = true;
    }

    public static void Error(Token token, string message)
    {
        LaunchDebugger();
        if (token.TokenType == TokenType.EOF)
            Report(token.Line, " at end", message);
        else
            Report(token.Line, $" at '{token.Lexeme}'", message);
    }

    public static void RuntimeError(RuntimeException ex)
    {
        LaunchDebugger();
        Console.Error.WriteLine($"{ex.Message}\n[line {ex.Token.Line}]");
        HasRuntimeError = true;
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void LaunchDebugger()
    {
        if (System.Diagnostics.Debugger.Launch())
            System.Diagnostics.Debugger.Break();
    }
}
