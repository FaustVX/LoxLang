namespace LoxLang.Core;

public class Token
{
    public required TokenType TokenType { get; init; }
    public required SubString Lexeme { get; init; }
    public object? Literal { get; init; }
    public required int Line { get; init; }

    public Token()
    {
        
    }

    public static Token EOF(int line)
        => new()
        {
            TokenType = TokenType.EOF,
            Lexeme = "",
            Line = line,
        };

    public override string ToString()
    {
        return $"[{Line}]{TokenType} {Lexeme} {Literal}";
    }
}
