using System.Diagnostics;

namespace OpenAnalogKeyboardDriver;

public static class Logger
{
    private static readonly long Timestamp0 = Stopwatch.GetTimestamp();

    public static void Debug(string message, string? moduleName = null)
    {
#if DEBUG
        Log(LogSeverity.Debug, message, moduleName);
#endif
    }

    public static void Info(string message, string? moduleName = null) => Log(LogSeverity.Info, message, moduleName);

    public static void Warn(string message, string? moduleName = null) => Log(LogSeverity.Warn, message, moduleName);

    public static void Error(string message, string? moduleName = null) => Log(LogSeverity.Error, message, moduleName);

    private static void Log(LogSeverity severity, string message, string? moduleName)
    {
        // Timestamp
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write(TimeSpan.FromSeconds((Stopwatch.GetTimestamp() - Timestamp0) * 1d / Stopwatch.Frequency).ToString(@"hh\:mm\:ss\.ffffff"));

        // Severity
        Console.Write("  ");
        Console.ForegroundColor = severity switch
        {
            LogSeverity.Debug => ConsoleColor.Gray,
            LogSeverity.Info => ConsoleColor.Green,
            LogSeverity.Warn => ConsoleColor.Yellow,
            LogSeverity.Error => ConsoleColor.Red,
            _ => ConsoleColor.White
        };
        Console.Write(severity.ToString().ToUpper().PadRight(5));

        // Module
        Console.Write("  ");
        Console.ForegroundColor = ConsoleColor.Gray;
        if (moduleName is null)
            moduleName = new StackTrace().GetFrames().First(x => x.GetMethod()?.DeclaringType != typeof(Logger)).GetMethod()!.DeclaringType!.Name;

        if (moduleName.Length > 20)
            moduleName = moduleName[..9] + ".." + moduleName[^9..];

        Console.Write(moduleName.PadRight(20));

        // Message
        Console.Write("  ");
        Console.WriteLine(message);
    }
}

public enum LogSeverity
{
    Debug,
    Info,
    Warn,
    Error
}