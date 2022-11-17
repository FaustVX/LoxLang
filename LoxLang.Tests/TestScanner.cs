using LoxLang.Core;

namespace LoxLang.Tests;

[TestClass()]
public class TestScanner
{
    [TestMethod]
    public void TestSingleBang()
    {
        var scanner = new Scanner("!");
        var tokens = scanner.ScanTokens();
        AssertErrors();
        switch (tokens)
        {
            case not [{TokenType: Core.TokenType.BANG}, {TokenType: Core.TokenType.EOF}]:
                Assert.Fail();
                break;
        }
    }

    [TestMethod]
    public void TestBangEquals()
    {
        var scanner = new Scanner("!=");
        var tokens = scanner.ScanTokens();
        AssertErrors();
        switch (tokens)
        {
            case not [{TokenType: Core.TokenType.BANG_EQUAL}, {TokenType: Core.TokenType.EOF}]:
                Assert.Fail();
                break;
        }
    }

    [TestMethod]
    public void TestSingleComment()
    {
        var scanner = new Scanner("// this is a comment");
        var tokens = scanner.ScanTokens();
        AssertErrors();
        switch (tokens)
        {
            case not [{TokenType: Core.TokenType.EOF}]:
                Assert.Fail();
                break;
        }
    }

    [TestMethod]
    public void TestBraces_Comment()
    {
        var scanner = new Scanner("(( )){} // grouping stuff");
        var tokens = scanner.ScanTokens();
        AssertErrors();
        switch (tokens)
        {
            case not [{TokenType: Core.TokenType.LEFT_PAREN}, {TokenType: TokenType.LEFT_PAREN}, {TokenType: TokenType.RIGHT_PAREN}, {TokenType: TokenType.RIGHT_PAREN}, {TokenType: TokenType.LEFT_BRACE}, {TokenType: TokenType.RIGHT_BRACE}, {TokenType: TokenType.EOF}]:
                Assert.Fail();
                break;
        }
    }

    [TestMethod]
    public void TestOperators_Comment()
    {
        var scanner = new Scanner("!*+-/=<> <= == // operators");
        var tokens = scanner.ScanTokens();
        AssertErrors();
        switch (tokens)
        {
            case not [{TokenType: Core.TokenType.BANG}, {TokenType: TokenType.STAR}, {TokenType: TokenType.PLUS}, {TokenType: TokenType.MINUS}, {TokenType: TokenType.SLASH}, {TokenType: TokenType.EQUAL}, {TokenType: TokenType.LESS}, {TokenType: TokenType.GREATER}, {TokenType: TokenType.LESS_EQUAL}, {TokenType: TokenType.EQUAL_EQUAL}, {TokenType: TokenType.EOF}]:
                Assert.Fail();
                break;
        }
    }

    [TestMethod]
    public void TestString()
    {
        var scanner = new Scanner("\"Hello\"");
        var tokens = scanner.ScanTokens();
        AssertErrors();
        switch (tokens)
        {
            case not [{TokenType: Core.TokenType.STRING, Literal: "Hello"}, {TokenType: TokenType.EOF}]:
                Assert.Fail();
                break;
        }
    }

    [TestMethod]
    public void TestUnfinishedString()
    {
        var scanner = new Scanner("\"Hello");
        var tokens = scanner.ScanTokens();
        AssertNotErrors();
    }

    [TestMethod]
    public void TestInteger()
    {
        var scanner = new Scanner("123");
        var tokens = scanner.ScanTokens();
        AssertErrors();
        switch (tokens)
        {
            case not [{TokenType: Core.TokenType.NUMBER, Literal: 123d}, {TokenType: TokenType.EOF}]:
                Assert.Fail();
                break;
        }
    }

    [TestMethod]
    public void TestDouble()
    {
        var scanner = new Scanner("123.45");
        var tokens = scanner.ScanTokens();
        AssertErrors();
        switch (tokens)
        {
            case not [{TokenType: Core.TokenType.NUMBER, Literal: 123.45d}, {TokenType: TokenType.EOF}]:
                Assert.Fail();
                break;
        }
    }

    [TestMethod]
    public void TestNumberLeadingDot()
    {
        var scanner = new Scanner(".45");
        var tokens = scanner.ScanTokens();
        AssertErrors();
        switch (tokens)
        {
            case not [{TokenType: TokenType.DOT}, {TokenType: TokenType.NUMBER, Literal: 45d}, {TokenType: TokenType.EOF}]:
                Assert.Fail();
                break;
        }
    }

    [TestMethod]
    public void TestNumberTrailingDot()
    {
        var scanner = new Scanner("123.");
        var tokens = scanner.ScanTokens();
        AssertErrors();
        switch (tokens)
        {
            case not [{TokenType: TokenType.NUMBER, Literal: 123d}, {TokenType: TokenType.DOT}, {TokenType: TokenType.EOF}]:
                Assert.Fail();
                break;
        }
    }

    [TestMethod]
    public void TestNegative()
    {
        var scanner = new Scanner("-123.45");
        var tokens = scanner.ScanTokens();
        AssertErrors();
        switch (tokens)
        {
            case not [{TokenType: TokenType.MINUS}, {TokenType: Core.TokenType.NUMBER, Literal: 123.45d}, {TokenType: TokenType.EOF}]:
                Assert.Fail();
                break;
        }
    }

    private void AssertErrors()
    {
        if (Lox.HasError)
            Assert.Fail();
        Lox.HasError = false;
    }

    private void AssertNotErrors()
    {
        if (!Lox.HasError)
            Assert.Fail();
        Lox.HasError = false;
    }
}
