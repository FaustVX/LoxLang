namespace LoxLang.Core;

public sealed partial class Interpreter : IStmtVisitor<Void>
{
    Void IStmtVisitor<Void>.Visit(ExprStmt stmt)
    {
        stmt.Expr.Accept(this);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(PrintStmt stmt)
    {
        Console.WriteLine(Stringify(stmt.Expr.Accept(this)));
        return default;
    }

    Void IStmtVisitor<Void>.Visit(VariableStmt stmt)
    {
        var value = stmt.Initializer.Accept(this);
        CurrentEnv.Define(stmt.Name, value);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(BlockStmt stmt)
    {
        ExecuteBlock(stmt.Statements, new(CurrentEnv));
        return default;
    }

    Void IStmtVisitor<Void>.Visit(IfStmt stmt)
    {
        if (IsTruthy(stmt.Condition.Accept(this)))
            stmt.Then.Accept(this);
        else
            stmt.Else?.Accept(this);
        return default;
    }

    Void IStmtVisitor<Void>.Visit(WhileStmt stmt)
    {
        try
        {
            while (IsTruthy(stmt.Condition.Accept(this)))
                stmt.Body.Accept(this);
        }
        catch (BreakControlFlowException)
        {
            return default;
        }
        return default;
    }

    Void IStmtVisitor<Void>.Visit(FunctionStmt stmt)
    {
        CurrentEnv.DefineFun(new Function(stmt, CurrentEnv, false));
        return default;
    }

    Void IStmtVisitor<Void>.Visit(ReturnStmt stmt)
    {
        var value = stmt.Expr?.Accept(this);
        throw new ReturnControlFlowException(value);
    }

    Void IStmtVisitor<Void>.Visit(BreakStmt stmt)
        => throw new BreakControlFlowException();

    Void IStmtVisitor<Void>.Visit(ClassStmt stmt)
    {
        var super = stmt.Super?.Accept(this);
        if (super is not (Class or null))
            throw new RuntimeException(stmt.Super!.Name, "Superclass must be a class.");
        var superClass = (Class?)super;
        CurrentEnv.Define(stmt.Name, null);
        if (superClass is not null)
        {
            CurrentEnv = new(CurrentEnv);
            CurrentEnv.DefineFun(superClass);
        }
        var methods = stmt.Methods.ToDictionary(static f => f.Name.Lexeme.ToString(), f => new Function(f, CurrentEnv, f.Name.Lexeme.ToString() == "init"));
        var @class = new Class(stmt.Name, superClass, methods);
        if (superClass is not null)
            CurrentEnv = CurrentEnv.Enclosing!;
        CurrentEnv.Assign(stmt.Name, @class);
        return default;
    }
}
