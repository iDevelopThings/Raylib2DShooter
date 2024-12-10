
using RLShooter.Common.Utils.Collections;

namespace RLShooter.App.Logging;

public class Logs {
    private static readonly Dictionary<string, Logger> loggers = new();

    private static readonly List<ILogWriter> globalWriters = new() {
        new ConsoleLogWriter(),
    };

    public static readonly Logger Global = Get(nameof(Global));


    public static IEnumerable<ILogWriter> GetGlobalWriters() {
        return globalWriters;
    }

    public static void AddGlobalWriter(ILogWriter writer) {
        Logs.globalWriters.Add(writer);
    }

    public static bool RemoveGlobalWriter(ILogWriter writer) {
        // lock (Logs.readWriteLock.BeginWriteBlock())
        return globalWriters.Remove(writer);
    }

    public static void ClearGlobalWriters() {
        globalWriters.Clear();
    }


    public static Logger Get(string n) {
        return loggers.GetOrAdd(n, () => new Logger(n));
    }

    public static T Get<T>(string n) where T : Logger, new() {
        return (T) loggers.GetOrAdd(n, () => new T() {Name = n});
    }

    public static bool DestroyLogger(string name) {
        return loggers.Remove(name);
    }

    public static void DestroyAll() => loggers.Clear();
}