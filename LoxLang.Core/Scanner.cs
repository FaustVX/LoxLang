using System.Globalization;

namespace LoxLang.Core;

public class Scanner
{
    public Scanner(string source)
        => Source = source;

    public SubString Source { get; }
    private List<Token> Tokens { get; } = new();
    private bool IsAtEnd
        => current >= Source.Length;
    private char CurrentChar
        => Source[current];
    private char NextChar
        => Source[current + 1];
    private SubString CurrentSub
        => Source[start..current];
    private int start = 0;
    private int current = 0;
    private int line = 1;

    public IReadOnlyList<Token> ScanTokens()
    {
        while (!IsAtEnd)
        {
            start = current;
            ScanToken();
        }
        Tokens.Add(Token.EOF(line));
        return Tokens;
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            case '(':
                AddToken(TokenType.LEFT_PAREN);
                break;
            case ')':
                AddToken(TokenType.RIGHT_PAREN);
                break;
            case '{':
                AddToken(TokenType.LEFT_BRACE);
                break;
            case '}':
                AddToken(TokenType.RIGHT_BRACE);
                break;
            case ',':
                AddToken(TokenType.COMMA);
                break;
            case '.':
                AddToken(TokenType.DOT);
                break;
            case '-':
                AddToken(MatchCurrent('-') ? TokenType.MINUS_MINUS : MatchCurrent('=') ? TokenType.MINUS_EQUAL : TokenType.MINUS);
                break;
            case '+':
                AddToken(MatchCurrent('+') ? TokenType.PLUS_PLUS : MatchCurrent('=') ? TokenType.PLUS_EQUAL : TokenType.PLUS);
                break;
            case ';':
                AddToken(TokenType.SEMICOLON);
                break;
            case ':':
                AddToken(TokenType.COLON);
                break;
            case '*':
                AddToken(MatchCurrent('=') ? TokenType.STAR_EQUAL : TokenType.STAR);
                break;
            case '!':
                AddToken(MatchCurrent('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                break;
            case '=':
                AddToken(MatchCurrent('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                break;
            case '<':
                AddToken(MatchCurrent('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                break;
            case '>':
                AddToken(MatchCurrent('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                break;
            case '%':
                AddToken(MatchCurrent('=') ? TokenType.PERCENT_EQUAL : TokenType.PERCENT);
                break;
            case '/':
                if (MatchCurrent('/'))
                    while (Peek() != '\n' && !IsAtEnd)
                        Advance();
                else if (MatchCurrent('='))
                    AddToken(TokenType.SLASH_EQUAL);
                else
                    AddToken(TokenType.SLASH);
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore whitespace.
                break;
            case '\n':
                line++;
                break;
            case '"':
                String();
                break;
            default:
                if (IsDigit(c))
                    Number();
                else if (IsAlpha(c))
                    Identifier();
                else
                    Lox.Error(line, $"Unexpected character: {c}");
                break;
        }
    }

    private void Identifier()
    {
        while (IsAlphaNumeric(Peek()))
            Advance();
        AddToken(TokenType.IDENTIFIER);
    }

    private bool IsDigit(char c)
        => c is >= '0' and <= '9';

    private bool IsAlpha(char c)
        => c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '_';

    private bool IsAlphaNumeric(char c)
        => IsDigit(c) || IsAlpha(c);

    private void Number()
    {
        while (IsDigit(Peek()))
            Advance();
        if (Peek() is '.' && IsDigit(PeekNext()))
        {
            Advance();
            while (IsDigit(Peek()))
                Advance();
        }
        AddToken(TokenType.NUMBER, double.Parse(CurrentSub.ToString(), CultureInfo.InvariantCulture));
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd)
        {
            if (Peek() == '\n')
                line++;
            Advance();
        }
        if (IsAtEnd)
        {
            Lox.Error(line, "Unterminated string");
            return;
        }
        Advance();
        AddToken(TokenType.STRING, CurrentSub[1..^1].ToString());
    }

    private char Advance()
        => Source[current++];

    private bool MatchCurrent(char next)
    {
        if (IsAtEnd || CurrentChar != next)
            return false;
        current++;
        return true;
    }

    private char Peek()
     => IsAtEnd ? '\0' : CurrentChar;

    private char PeekNext()
     => current + 1 >= Source.Length ? '\0' : NextChar;

    private void AddToken(TokenType type, object literal = null!)
    {
        var lexeme = CurrentSub;
        if (type is TokenType.IDENTIFIER)
            type = CurrentSub.ToString() switch
            {
                "and" => TokenType.AND,
                "break" => TokenType.BREAK,
                "class" => TokenType.CLASS,
                "else" => TokenType.ELSE,
                "false" => TokenType.FALSE,
                "for" => TokenType.FOR,
                "fun" => TokenType.FUN,
                "if" => TokenType.IF,
                "loop" => TokenType.LOOP,
                "nil" => TokenType.NIL,
                "or" => TokenType.OR,
                "print" => TokenType.PRINT,
                "return" => TokenType.RETURN,
                "super" => TokenType.SUPER,
                "this" => TokenType.THIS,
                "true" => TokenType.TRUE,
                "var" => TokenType.VAR,
                "while" => TokenType.WHILE,
                _ => type,
            };
        Token token = literal switch
        {
            null => new Token()
            {
                TokenType = type,
                Lexeme = lexeme,
                Line = line,
            },
            var obj => new LiteralToken()
            {
                TokenType = type,
                Lexeme = lexeme,
                Line = line,
                Literal = obj,
            },
        };
        Tokens.Add(token);
    }
}
