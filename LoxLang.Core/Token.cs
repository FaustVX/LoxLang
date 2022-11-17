namespace LoxLang.Core;

public class Token
{
    public required TokenType TokenType { get; init; }
    public required SubString Lexeme { get; init; }
    public required int Line { get; init; }

    public static Token EOF(int line)
        => new()
        {
            TokenType = TokenType.EOF,
            Lexeme = "",
            Line = line,
        };

    public override string ToString()
        => $"[{Line}]{TokenType} {Lexeme}";
}

public class LiteralToken : Token
{
    public required object Literal { get; init; }

    public override string ToString()
        => $"[{Line}]{TokenType} {Lexeme} {Literal}";
}
