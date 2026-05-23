using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace OpenAnalogKeyboardDriver;

public static class DriverUtils
{
    public static void InitializeThread()
    {
        Thread.CurrentThread.Priority = ThreadPriority.Highest;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
    }

    public static void LogInformation()
    {
        string version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        Logger.Info($"OS: {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture.ToString().ToLower()}) Driver: {version} ({RuntimeInformation.FrameworkDescription})");
    }
}