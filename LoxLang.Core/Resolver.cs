namespace LoxLang.Core;

public sealed partial class Resolver
{
    private enum FunctionType
    {
        None,
        Function,
        Method,
        Initializer,
    }

    private enum ClassType
    {
        None,
        Class,
        SubClass,
    }

    private enum LoopType
    {
        None,
        Loop,
    }

    private enum StaticType
    {
        None,
        Static,
    }

    private readonly Interpreter _interpreter;
    private readonly Stack<Dictionary<string, (bool defined, bool accessed, Token name)>> _scopes = new();
    private FunctionType _currentFunction = FunctionType.None;
    private ClassType _currentClass = ClassType.None;
    private LoopType _currentLoop = LoopType.None;
    private StaticType _currentStatic = StaticType.None;

    public Resolver(Interpreter interpreter)
        => _interpreter = interpreter;

    public void Resolve(List<Stmt> statements)
    {
        foreach (var stmt in statements)
            stmt.Accept(this);
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        var i = 0;
        foreach (var scope in _scopes)
        {
            if (scope.TryGetValue(name.Lexeme.ToString(), out var infos))
            {
                scope[name.Lexeme.ToString()] = infos with { accessed = true };
                _interpreter.Resolve(expr, i);
                return;
            }
            i++;
        }
    }

    private void Declare(Token name)
    {
        if (_scopes.TryPeek(out var scope))
        {
            if (scope.ContainsKey(name.Lexeme.ToString()))
                Lox.Error(name, "Already a variable with this name in this scope.");
            scope[name.Lexeme.ToString()] = (false, false, name);
        }
    }

    private void Define(Token name)
    {
        if (_scopes.TryPeek(out var scope))
        {
            var infos = scope[name.Lexeme.ToString()];
            scope[name.Lexeme.ToString()] = infos with { defined = true };
        }
    }

    private void ReportVariableUsage()
    {
        if (_scopes.TryPeek(out var scope))
            foreach (var (name, infos) in scope)
                if (!infos.accessed)
                    Lox.Warning(infos.name, "Variable not used.");
    }

    private Scope CreateScope()
    {
        var scope = new Scope(this);
        scope.Push();
        return scope;
    }

    private readonly struct Scope : IDisposable
    {
        private readonly Resolver _resolver;

        public Dictionary<string, (bool defined, bool accessed, Token name)> Actual { get; } = new();

        public Scope(Resolver resolver)
            => _resolver = resolver;

        public void Push()
            => _resolver._scopes.Push(Actual);

        void IDisposable.Dispose()
            => Pop();

        public void Pop()
            => _resolver._scopes.Pop();
    }
}
