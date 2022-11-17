using LoxLang.Core;

internal static class Program
{
    private static void Main(string[] args)
    {
        switch (args)
        {
            case [var filePath] when new FileInfo(filePath) is { Exists: true, Extension: ".lox" } file:
                RunFile(file);
                break;
            case []:
                RunPrompt();
                break;
            default:
                System.Console.WriteLine("Usage: jlox [script]");
                Environment.Exit(64); // https://craftinginterpreters.com/scanning.html#the-interpreter-framework https://www.freebsd.org/cgi/man.cgi?query=sysexits&apropos=0&sektion=0&manpath=FreeBSD+4.3-RELEASE&format=html
                break;
        }
    }

    private static void RunFile(FileInfo file)
    {
        Lox.Run(File.ReadAllText(file.FullName));
        if (Lox.HasError)
            Environment.Exit(65);
    }

    private static void RunPrompt()
    {
        while (true)
        {
            System.Console.Write("> ");
            if (Console.ReadLine() is string { Length: > 0 } s)
            {
                Lox.Run(s);
                Lox.HasError = false;
            }
            else
                break;
        }
    }
}
