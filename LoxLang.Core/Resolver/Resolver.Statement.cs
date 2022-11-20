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
        (var enclosint, _currentLoop) = (_currentLoop, LoopType.Loop);
        stmt.Condition.Accept(this);
        stmt.Body.Accept(this);
        _currentLoop = enclosint;
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
        stmt.Expr?.Accept(this);
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
        Declare(stmt.Name);
        Define(stmt.Name);

        using (var scope = CreateScope())
        {
            scope.Actual["this"] = (true, true, stmt.Name);
            foreach (var func in stmt.Methods)
            {
                var type = FunctionType.Method;
                ResolveFunction(func, type);
            }
        }

        return default;
    }

    private void ResolveFunction(FunctionStmt stmt, FunctionType type)
    {
        (var enclosing, _currentFunction) = (_currentFunction, type);
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
        _currentFunction = enclosing;
    }
}
