namespace LoxLang.Core;

public static class Lox
{
    public static bool HasError { get; set; }

    public static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();
        var parser = new Parser(tokens);
        var expr = parser.Parse();

        if (HasError || expr is null)
            return;
        Console.WriteLine(new ASTPrinterRPN().Print(expr));
    }

    public static void Error(int line, string message)
        => Report(line, "", message);

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line: {line}] Error{where}: {message}");
        HasError = true;
    }

    internal static void Error(Token token, string message)
    {
        if (token.TokenType == TokenType.EOF)
            Report(token.Line, " at end", message);
        else
            Report(token.Line, $" at '{token.Lexeme}'", message);
    }
}
