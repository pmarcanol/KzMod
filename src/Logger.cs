using DuckGame;
using KzDuckMods;
using System;
using System.Diagnostics;
using System.IO;

public static class Logger
{
    public static void LogToFile(string message)
    {
        string callerClassName = GetCallingClassName();

        string logFilename = callerClassName.ToLower() + ".txt";
        string logPath = Mod.GetPath<KzMod>(logFilename);

        Directory.CreateDirectory(Path.GetDirectoryName(logPath));

        using (StreamWriter writer = new StreamWriter(logPath, true))
        {
            writer.WriteLine($"{DateTime.Now}: {message}");
        }
    }

    private static string GetCallingClassName()
    {
        StackTrace stackTrace = new StackTrace();
        for (int i = 1; i < stackTrace.FrameCount; i++)
        {
            var method = stackTrace.GetFrame(i).GetMethod();
            var declaringType = method.DeclaringType;
            if (declaringType != typeof(Logger) && declaringType != null)
            {
                return declaringType.Name;
            }
        }
        return "unknown";
    }
}
