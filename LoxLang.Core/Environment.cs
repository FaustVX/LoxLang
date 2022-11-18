namespace LoxLang.Core;

public class Environment
{
    private readonly Environment? _enclosing;
    private readonly Dictionary<string, object?> _values = new();

    public Environment()
        => _enclosing = null;

    public Environment(Environment enclosing)
        => _enclosing = enclosing;

    public void Define(Token name, object? value)
    {
        if (!_values.ContainsKey(name.Lexeme.ToString()))
            _values[name.Lexeme.ToString()] = value;
        else
            throw new RuntimeException(name, $"variable '{name.Lexeme}' already defined");
    }

    public object? Get(Token token)
    {
        if (_values.TryGetValue(token.Lexeme.ToString(), out var value))
            return value;
        if (_enclosing is {} env)
            return env.Get(token);
        throw new RuntimeException(token, $"Undefined variable '{token.Lexeme}'.");
    }

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme.ToString()))
            _values[name.Lexeme.ToString()] = value;
        else if (_enclosing is {} env)
            env.Assign(name, value);
        else
            throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'");
    }
}
