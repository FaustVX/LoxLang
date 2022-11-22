namespace LoxLang.Core;

public sealed partial class Resolver : IStmtVisitor<Void>
{
    Void IStmtVisitor<Void>.Visit(ExprStmt stmt)
    {
        stmt.Expr.Accept(this);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(PrintStmt stmt)
    {
        stmt.Expr.Accept(this);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(VariableStmt stmt)
    {
        Declare(stmt.Name);
        stmt.Initializer.Accept(this);
        Define(stmt.Name);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(BlockStmt stmt)
    {
        using (_ = CreateScope())
        {
            Resolve(stmt.Statements);
            ReportVariableUsage();
        }
        return default;
    }

    Void IStmtVisitor<Void>.Visit(IfStmt stmt)
    {
        stmt.Condition.Accept(this);
        stmt.Then.Accept(this);
        stmt.Else?.Accept(this);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(WhileStmt stmt)
    {
        using (_ = Switch(ref _currentLoop, LoopType.Loop))
        {
            stmt.Condition.Accept(this);
            stmt.Body.Accept(this);
        }
        return default;
    }

    Void IStmtVisitor<Void>.Visit(FunctionStmt stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.Function);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(ReturnStmt stmt)
    {
        if (_currentFunction is FunctionType.None)
            Lox.Error(stmt.keyword, "Can't return from top-level code.");
        if (stmt.Expr is {} value)
        {
            if (_currentFunction is FunctionType.Initializer)
                Lox.Error(stmt.keyword, "Can't return a value from an initializer.");
            stmt.Expr?.Accept(this);
        }
        return default;
    }

    Void IStmtVisitor<Void>.Visit(BreakStmt stmt)
    {
        if (_currentLoop is LoopType.None)
            Lox.Error(stmt.keyword, "Can't break out of a loop.");
        return default;
    }

    Void IStmtVisitor<Void>.Visit(ClassStmt stmt)
    {
        using (_ = Switch(ref _currentClass, ClassType.Class))
        {
            Declare(stmt.Name);
            Define(stmt.Name);

            var superScope = new Scope(this);
            if (stmt.Super is {} super)
            {
                if (super.Name.Lexeme.ToString() == stmt.Name.Lexeme.ToString())
                    Lox.Error(super.Name, "A class can't inherit from itself.");
                _currentClass = ClassType.SubClass;
                super.Accept(this);
                superScope.Push();
                superScope.Actual["super"] = (true, true, super.Name);
            }


            var group = stmt.Methods.GroupBy(static m => m.IsStatic).ToDictionary(static g => g.Key, static g => g.AsEnumerable());

            if (group.TryGetValue(true, out var _static))
                foreach (var func in _static)
                {
                    var type = func.Name.Lexeme.ToString() == "init" ? FunctionType.Initializer : FunctionType.Method;
                    ResolveFunction(func, type);
                }
            using (var scope = CreateScope())
            {
                scope.Actual["this"] = (true, true, stmt.Name);
                if (group.TryGetValue(false, out var methods))
                    foreach (var func in methods)
                    {
                        var type = func.Name.Lexeme.ToString() == "init" ? FunctionType.Initializer : FunctionType.Method;
                        ResolveFunction(func, type);
                    }
            }

            if (stmt.Super is not null)
                superScope.Pop();
        }

        return default;
    }

    private void ResolveFunction(FunctionStmt stmt, FunctionType type)
    {
        using (_ = Switch(ref _currentStatic, stmt.IsStatic ? StaticType.Static : _currentStatic))
        using (_ = Switch(ref _currentFunction, type))
        using (_ = CreateScope())
        {
            foreach (var param in stmt.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(stmt.Body);
            ReportVariableUsage();
        }
    }
}
