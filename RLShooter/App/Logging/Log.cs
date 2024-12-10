
namespace RLShooter.App.Logging;

public static class Log
{
    private static Logger global = Logs.Get("Global");

    public static void Trace(string text)                       => global.Trace(text);
    public static void Trace(string text, params object[] args) => global.Trace(text, args);

    public static void Debug(string text)                       => global.Debug(text);
    public static void Debug(string text, params object[] args) => global.Debug(text, args);

    public static void Info(string text)                       => global.Info(text);
    public static void Info(string text, params object[] args) => global.Info(text, args);

    public static void Warning(string text)                       => global.Warning(text);
    public static void Warning(string text, params object[] args) => global.Warning(text, args);

    public static void Error(string text)                       => global.Error(text);
    public static void Error(string text, params object[] args) => global.Error(text, args);

    public static void Fatal(string text)                       => global.Fatal(text);
    public static void Fatal(string text, params object[] args) => global.Fatal(text, args);
}