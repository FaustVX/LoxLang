using LoxLang.Core;

namespace LoxLang.Tests;

[TestClass]
public class TestASTPrinter
{
    [TestMethod]
    public void Test1()
    {
        var expr = new BinaryExpr(new LiteralExpr(1), new() { TokenType = TokenType.PLUS, Lexeme = "+", Line = 0 }, new LiteralExpr(2));
        var printer = new ASTPrinterRPN();
        Assert.AreEqual("1 2 +", printer.Print(expr));
    }

    [TestMethod]
    public void Test2()
    {
        var expr = new BinaryExpr(
            new UnaryExpr(
                new Token() { Lexeme = "-", Line = 1, TokenType = TokenType.MINUS },
                new LiteralExpr(123)),
            new Token() { Lexeme = "*", Line = 1, TokenType = TokenType.STAR },
            new GroupExpr(
                new LiteralExpr(45.67)));
        var printer = new ASTPrinterRPN();
        Assert.AreEqual("123 - (45,67) *", printer.Print(expr));
    }
}
