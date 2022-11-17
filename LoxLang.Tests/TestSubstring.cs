using LoxLang.Core;

namespace LoxLang.Tests;

[TestClass]
public class TestSubstring
{
    [TestMethod]
    public void ImplicitConversion()
    {
        var s = "String";
        var ss = (SubString)s;
        Assert.AreEqual(0, ss.Start);
        Assert.AreEqual(s.Length, ss.Length);
    }

    private static void TestSubStringRange(string s, Range range)
    {
        var ss = (SubString)s;
        Assert.AreEqual(s[range], ss[range].ToString());
    }

    private static void TestSubStringRange(string s, Range range1, Range range2)
    {
        var ss = (SubString)s;
        ss = ss[range1][range2];
        Assert.AreEqual(s[range1][range2], ss.ToString());
    }

    [TestMethod]
    public void GetRangePositivePositive()
        => TestSubStringRange("String", 1..2);

    [TestMethod]
    public void GetRangePositiveNegative()
        => TestSubStringRange("String", 1..^2);

    [TestMethod]
    public void GetRangeNegativePositive()
        => TestSubStringRange("String", ^6..2);

    [TestMethod]
    public void GetRangeNegativeNegative()
        => TestSubStringRange("String", ^6..^2);

    [TestMethod]
    public void GetRangePositiveNegativeDouble()
        => TestSubStringRange("String", 1..^1, 1..^1);
}
