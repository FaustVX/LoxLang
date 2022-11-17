namespace LoxLang.Core;

public static class Lox
{
    public static bool HasError { get; set; }

    public static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        foreach (var token in tokens)
        {
            System.Console.WriteLine(token);
        }
    }

    public static void Error(int line, string message)
        => Report(line, "", message);

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine($"[line: {line}] Error{where}: {message}");
        HasError = true;
    }
}
