namespace LoxLang.Core;

public sealed partial class Interpretor : IStmtVisitor<Void>
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
        var value = stmt.Initializer?.Accept(this);
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
        CurrentEnv.DefineFun(new Function(stmt, CurrentEnv));
        return default;
    }

    Void IStmtVisitor<Void>.Visit(ReturnStmt stmt)
    {
        var value = stmt.Expr?.Accept(this);
        throw new ReturnControlFlowException(value);
    }

    Void IStmtVisitor<Void>.Visit(BreakStmt stmt)
        => throw new BreakControlFlowException();
}
