namespace LoxLang.Core;

public class Environment
{
    public Environment? Enclosing { get; }
    private readonly Dictionary<string, object?> _values = new();

    public Environment()
        => Enclosing = null;

    public Environment(Environment enclosing)
        => Enclosing = enclosing;

    public void Define(Token name, object? value)
    {
        if (!_values.ContainsKey(name.Lexeme.ToString()))
            _values[name.Lexeme.ToString()] = value;
        else
            throw new RuntimeException(name, $"variable '{name.Lexeme}' already defined");
    }

    public void DefineFun(NativeCallable function)
    {
        if (!_values.ContainsKey(function.Name))
            _values[function.Name] = function;
    }

    public void DefineFun(DefinedCallable function)
    {
        if (!_values.ContainsKey(function.Name))
            _values[function.Name] = function;
        else
            throw new RuntimeException(function.NameToken, $"variable '{function.Name}' already defined");
    }

    public void DefineFun(Instance instance)
        => _values["this"] = instance;

    public void DefineFun(Class c)
        => _values["super"] = c;

    public object? Get(Token token)
    {
        if (_values.TryGetValue(token.Lexeme.ToString(), out var value))
            return value;
        if (Enclosing is {} env)
            return env.Get(token);
        throw new RuntimeException(token, $"Undefined variable '{token.Lexeme}'.");
    }

    public void Assign(Token name, object? value)
    {
        if (_values.ContainsKey(name.Lexeme.ToString()))
            _values[name.Lexeme.ToString()] = value;
        else if (Enclosing is {} env)
            env.Assign(name, value);
        else
            throw new RuntimeException(name, $"Undefined variable '{name.Lexeme}'");
    }

    public object? GetAt(int distance, string name)
        => Ancestor(distance)._values[name];

    private Environment Ancestor(int distance)
    {
        var env = this;
        for (int i = 0; i < distance; i++)
            env = env?.Enclosing;
        return env!;
    }

    public void AssignAt(int distance, Token name, object? value)
        => Ancestor(distance)._values[name.Lexeme.ToString()] = value;
}
