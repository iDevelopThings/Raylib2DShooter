using System.Runtime.CompilerServices;

namespace RLShooter.App.Logging;

public interface ILogWriter : IDisposable
{
    public virtual bool WritesStrings => true;

    void Write(string message);

    void WriteLine(string message) => this.Write(message + Environment.NewLine);

    void Write(LogMessage message);

    void Log(LogMessage message) {
        if (!WritesStrings) {
            this.Write(message);
            return;
        }

        DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 4);
        interpolatedStringHandler.AppendFormatted(message.Timestamp.ToShortTimeString());
        interpolatedStringHandler.AppendLiteral(" [");
        interpolatedStringHandler.AppendFormatted<TraceLogLevel>(message.Severity);
        interpolatedStringHandler.AppendLiteral("] [");
        interpolatedStringHandler.AppendFormatted(message.Logger.Name);
        interpolatedStringHandler.AppendLiteral("] ");
        interpolatedStringHandler.AppendFormatted(message.Message);

        this.WriteLine(interpolatedStringHandler.ToStringAndClear());
    }

    void Flush();

    void Clear();
}

public class ConsoleLogWriter : ILogWriter
{
    public virtual bool WritesStrings => false;

    public void Dispose() { }

    public void Write(LogMessage message) {
        switch (message.Severity) {
            case TraceLogLevel.All:
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"[ALL] {message.Message}");
                Console.ResetColor();
                break;

            case TraceLogLevel.Trace:
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[TRACE] {message.Message}");
                Console.ResetColor();
                break;

            case TraceLogLevel.Debug:
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine($"[DEBUG] {message.Message}");
                Console.ResetColor();
                break;

            case TraceLogLevel.Info:
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[INFO] {message.Message}");
                Console.ResetColor();
                break;

            case TraceLogLevel.Warning:
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARNING] {message.Message}");
                Console.ResetColor();
                break;

            case TraceLogLevel.Error:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] {message.Message}");
                Console.ResetColor();
                break;

            case TraceLogLevel.Fatal:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FATAL] {message.Message}");
                Console.ResetColor();
                break;

            default:
                Console.WriteLine(message.Message);
                break;
        }

    }

    public void Write(string message) {
    }

    public void Flush() { }

    public void Clear() { }
}

public class RaylibLogWriter : ILogWriter
{
    public virtual bool WritesStrings => false;

    public void Dispose() { }

    public void Write(LogMessage message) {
        TraceLog((int)message.Severity, message.Message);
    }

    public void Write(string message) { }

    public void Flush() { }

    public void Clear() { }
}