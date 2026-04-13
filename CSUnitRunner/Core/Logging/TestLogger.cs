using System;
using System.IO;

namespace CSUnitRunner.Core.Logging;

internal static class TestLogger
{
    private static readonly string LogPath = "tests.log";
    private static readonly object Lock = new();

    static TestLogger()
    {
        Clear();
    }

    public static void Clear()
    {
        lock (Lock)
        {
            try { File.WriteAllText(LogPath, $"--- TEST SESSION STARTED AT {DateTime.Now} ---\n"); } catch { }
        }
    }

    public static void Info(string message)
    {
        Log(message);
    }

    public static void Event(string message)
    {
        Log(message);
        Console.WriteLine(message);
    }

    private static void Log(string message)
    {
        lock (Lock)
        {
            try { File.AppendAllText(LogPath, $"[{DateTime.Now:HH:mm:ss}] {message}\n"); } catch { }
        }
    }
}
