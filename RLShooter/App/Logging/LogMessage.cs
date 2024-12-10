
namespace RLShooter.App.Logging;

public struct LogMessage : IEquatable<LogMessage>
{
    public Logger        Logger;
    public TraceLogLevel Severity;
    public string        Message;
    public DateTime      Timestamp;

    public LogMessage(Logger logger, TraceLogLevel severity, string source, string message)
        : this() {
        Logger    = logger;
        Severity  = severity;
        Message   = "[" + source + "]: " + message;
        Timestamp = DateTime.Now;
    }

    public LogMessage(Logger logger, TraceLogLevel severity, string message)
        : this() {
        Logger    = logger;
        Severity  = severity;
        Message   = message;
        Timestamp = DateTime.Now;
    }

    public readonly override bool Equals(object obj) {
        return obj is LogMessage other && Equals(other);
    }

    public readonly bool Equals(LogMessage other) {
        return EqualityComparer<Logger>.Default.Equals(Logger, other.Logger) && Severity == other.Severity && Message == other.Message && Timestamp == other.Timestamp;
    }

    public readonly override int GetHashCode() {
        return HashCode.Combine<Logger, TraceLogLevel, string, DateTime>(Logger, Severity, Message, Timestamp);
    }

    public static bool operator ==(LogMessage left, LogMessage right) => left.Equals(right);

    public static bool operator !=(LogMessage left, LogMessage right) => !(left == right);
}