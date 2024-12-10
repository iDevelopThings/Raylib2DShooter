using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RLShooter.App.Logging;

public partial class StringFormatter {
    private const string Msvcrt    = "msvcrt";
    private const string Libc      = "libc";
    private const string LibSystem = "libSystem";

    /// <summary>
    /// Formats a message string using the specified format and arguments on Windows platform.
    /// </summary>
    /// <param name="buffer">Pointer to the buffer to write the formatted string.</param>
    /// <param name="format">Pointer to the format string.</param>
    /// <param name="args">Pointer to the arguments.</param>
    /// <returns>The number of characters written to the buffer.</returns>
    [LibraryImport(Msvcrt, EntryPoint = "vsprintf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int VsPrintFWindows(nint buffer, nint format, nint args);

    /// <summary>
    /// Formats a message string using the specified format and arguments on Windows platform.
    /// </summary>
    /// <param name="buffer">Pointer to the buffer to write the formatted string.</param>
    /// <param name="size">Size of the buffer.</param>
    /// <param name="format">A pointer to a null-terminated string that specifies the format of the output.</param>
    /// <param name="args">A pointer to a list of arguments.</param>
    /// <returns>
    /// The formatted message as a string. If the formatting fails, returns an empty string.
    /// </returns>
    [LibraryImport(Msvcrt, EntryPoint = "vsnprintf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int VsnPrintFWindows(nint buffer, nuint size, nint format, nint args);

    /// <summary>
    /// Formats a message string using the specified format and arguments on Linux platform.
    /// </summary>
    /// <param name="buffer">Pointer to the buffer to write the formatted string.</param>
    /// <param name="format">A pointer to a null-terminated string that specifies the format of the output.</param>
    /// <param name="args">A pointer to a list of arguments.</param>
    /// <returns>
    /// The formatted message as a string. If the formatting fails, returns an empty string.
    /// </returns>
    [LibraryImport(Libc, EntryPoint = "vsprintf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int VsPrintFLinux(nint buffer, nint format, nint args);

    /// <summary>
    /// Formats a message string using the specified format and arguments on Linux platform.
    /// </summary>
    /// <param name="buffer">A pointer to a buffer to store the formatted message.</param>
    /// <param name="size">The size of the buffer.</param>
    /// <param name="format">A pointer to a null-terminated string that specifies the format of the output.</param>
    /// <param name="args">A pointer to a list of arguments.</param>
    /// <returns>
    /// The number of characters written to the buffer (excluding the null terminator) if the formatting succeeds;
    /// otherwise, returns a negative value to indicate an error occurred.
    /// </returns>
    [LibraryImport(Libc, EntryPoint = "vsnprintf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int VsnPrintFLinux(nint buffer, nuint size, nint format, nint args);

    /// <summary>
    /// Formats a message string using the specified format and arguments on macOS platform.
    /// </summary>
    /// <param name="buffer">Pointer to the buffer to write the formatted string.</param>
    /// <param name="format">A pointer to a null-terminated string that specifies the format of the output.</param>
    /// <param name="args">A pointer to a list of arguments.</param>
    /// <returns>
    /// The formatted message as a string. If the formatting fails, returns an empty string.
    /// </returns>
    [LibraryImport(LibSystem, EntryPoint = "vasprintf")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
    private static partial int VasPrintFOsx(ref nint buffer, nint format, nint args);

    /// <summary>
    /// Retrieves a formatted message string using the specified format and arguments based on the current platform.
    /// </summary>
    /// <param name="format">A pointer to a null-terminated string that specifies the format of the output.</param>
    /// <param name="args">A pointer to a list of arguments.</param>
    /// <returns>
    /// The formatted message as a string. If the formatting fails, returns an empty string.
    /// </returns>
    public string GetMessage(nint format, nint args) {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            return this.WindowsPrintF(format, args);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
            if (nint.Size == 8) {
                return this.Linux64PrintF(format, args);
            }

            return this.Linux86PrintF(format, args);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
            return this.OsxPrintF(format, args);
        }

        return "This Platform is not supported!";
    }

    /// <summary>
    /// Formats a message string using the specified format and arguments on Windows platform.
    /// </summary>
    /// <param name="format">A pointer to a null-terminated string that specifies the format of the output.</param>
    /// <param name="args">A pointer to a list of arguments.</param>
    /// <returns>
    /// The formatted message as a string. If the formatting fails, returns an empty string.
    /// </returns>
    private string WindowsPrintF(nint format, nint args) {
        nint utf8Buffer = nint.Zero;

        try {
            // Get size of args.
            int byteSize = VsnPrintFWindows(nint.Zero, nuint.Zero, format, args) + 1;

            if (byteSize <= 1) {
                return string.Empty;
            }

            // Allocate buffer.
            utf8Buffer = Marshal.AllocHGlobal(byteSize);
            VsPrintFWindows(utf8Buffer, format, args);

            return Marshal.PtrToStringUTF8(utf8Buffer)!;
        }
        finally {
            Marshal.FreeHGlobal(utf8Buffer);
        }
    }

    /// <summary>
    /// Formats a message string using the specified format and arguments on Linux 64-bit platform.
    /// </summary>
    /// <param name="format">A pointer to a null-terminated string that specifies the format of the output.</param>
    /// <param name="args">A pointer to a list of arguments.</param>
    /// <returns>
    /// The formatted message as a string. If the formatting fails, returns an empty string.
    /// </returns>
    private string Linux64PrintF(nint format, nint args) {
        nint listPointer = nint.Zero;
        nint utf8Buffer  = nint.Zero;

        try {
            VaListLinux64 str = Marshal.PtrToStructure<VaListLinux64>(args);

            // Get size of args.
            listPointer = Marshal.AllocHGlobal(Marshal.SizeOf(str));
            Marshal.StructureToPtr(str, listPointer, false);
            int byteSize = VsnPrintFLinux(nint.Zero, nuint.Zero, format, listPointer) + 1;

            // Allocate buffer.
            Marshal.StructureToPtr(str, listPointer, false);
            utf8Buffer = Marshal.AllocHGlobal(byteSize);

            // Print result into buffer.
            VsPrintFLinux(utf8Buffer, format, listPointer);
            return Marshal.PtrToStringUTF8(utf8Buffer)!;
        }
        finally {
            Marshal.FreeHGlobal(listPointer);
            Marshal.FreeHGlobal(utf8Buffer);
        }
    }

    /// <summary>
    /// Formats a message string using the specified format and arguments on Linux platform.
    /// </summary>
    /// <param name="format">A pointer to a null-terminated string that specifies the format of the output.</param>
    /// <param name="args">A pointer to a list of arguments.</param>
    /// <returns>
    /// The formatted message as a string. If the formatting fails, returns an empty string.
    /// </returns>
    private string Linux86PrintF(nint format, nint args) {
        nint utf8Buffer = nint.Zero;

        try {
            // Get size of args.
            int byteSize = VsnPrintFLinux(nint.Zero, nuint.Zero, format, args) + 1;

            if (byteSize <= 1) {
                return string.Empty;
            }

            // Allocate buffer and print into buffer.
            utf8Buffer = Marshal.AllocHGlobal(byteSize);
            VsPrintFLinux(utf8Buffer, format, args);

            return Marshal.PtrToStringUTF8(utf8Buffer)!;
        }
        finally {
            Marshal.FreeHGlobal(utf8Buffer);
        }
    }

    /// <summary>
    /// Formats a message string using the specified format and arguments on macOS platform.
    /// </summary>
    /// <param name="format">A pointer to a null-terminated string that specifies the format of the output.</param>
    /// <param name="args">A pointer to a list of arguments.</param>
    /// <returns>
    /// The formatted message as a string. If the formatting fails, returns an empty string.
    /// </returns>
    private string OsxPrintF(nint format, nint args) {
        nint utf8Buffer = nint.Zero;

        try {
            // Get size of args.
            int byteSize = VasPrintFOsx(ref utf8Buffer, format, args);

            if (byteSize == -1) {
                return string.Empty;
            }

            return Marshal.PtrToStringUTF8(utf8Buffer) ?? string.Empty;
        }
        finally {
            Marshal.FreeHGlobal(utf8Buffer);
        }
    }

    /// <summary>
    /// Represents a structure for handling variadic arguments on the Linux 64-bit platform.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct VaListLinux64 {
        private uint _gpOffset;
        private uint _fpOffset;
        private nint _overflowArgsArea;
        private nint _regSaveArea;
    }
}

public class Logger {
    public string Name { get; set; }

    protected static bool bSetTraceCallback = false;

    public delegate bool          OnMessage(TraceLogLevel logLevel, string text);
    public static event OnMessage Message;

    protected static StringFormatter formatter = new();

    public Logger(string name) {
        Name = name;
        Init();
    }

    /// <summary>
    /// Initializes the logger by setting the trace log callback.
    /// </summary>
    public static unsafe void Init() {
        if (!bSetTraceCallback) {
            bSetTraceCallback = true;
            SetTraceLogCallback(TraceLogCallback);
        }
    }
    /// <inheritdoc cref="RaylibApi.TraceLog" />
    public static void TraceLog(TraceLogLevel logLevel, string text, params object[] args) {
        Raylib.TraceLog((int) logLevel, string.Format(text, args));
    }

    public LogMessage ConstructMessage(TraceLogLevel logLevel, string text, params object[] args) {
        return new LogMessage(this, logLevel, string.Format(text, args));
    }

    public void ConstructAndLogMessage(TraceLogLevel logLevel, string text, params object[] args) {
        var msg = ConstructMessage(logLevel, text, args);
        foreach (ILogWriter globalWriter in Logs.GetGlobalWriters())
            globalWriter.Write(msg);
    }

    public void Trace(string text)                       => ConstructAndLogMessage(TraceLogLevel.Trace, text);
    public void Trace(string text, params object[] args) => ConstructAndLogMessage(TraceLogLevel.Trace, string.Format(text, args));

    public void Debug(string text)                       => ConstructAndLogMessage(TraceLogLevel.Debug, text);
    public void Debug(string text, params object[] args) => ConstructAndLogMessage(TraceLogLevel.Debug, string.Format(text, args));

    public void Info(string text)                       => ConstructAndLogMessage(TraceLogLevel.Info, text);
    public void Info(string text, params object[] args) => ConstructAndLogMessage(TraceLogLevel.Info, string.Format(text, args));

    public void Warning(string text)                       => ConstructAndLogMessage(TraceLogLevel.Warning, text);
    public void Warning(string text, params object[] args) => ConstructAndLogMessage(TraceLogLevel.Warning, string.Format(text, args));

    public void Error(string text)                       => ConstructAndLogMessage(TraceLogLevel.Error, text);
    public void Error(string text, params object[] args) => ConstructAndLogMessage(TraceLogLevel.Error, string.Format(text, args));

    public void Fatal(string text)                       => ConstructAndLogMessage(TraceLogLevel.Fatal, text);
    public void Fatal(string text, params object[] args) => ConstructAndLogMessage(TraceLogLevel.Fatal, string.Format(text, args));

    /// <inheritdoc cref="RaylibApi.SetTraceLogLevel" />
    public static void SetTraceLogLevel(TraceLogLevel logLevel) {
        Raylib.SetTraceLogLevel((int) logLevel);
    }

    /// <summary>
    /// Callback method that is called whenever a new log message is generated.
    /// </summary>
    /// <param name="logLevel">The level of the log message.</param>
    /// <param name="text">The log message.</param>
    /// <param name="args">Additional arguments.</param>
    // [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    protected static unsafe void TraceLogCallback(int logLevel, byte* text, nint args) {
        var textPtr = new IntPtr(text);
        var textStr = Marshal.PtrToStringUTF8(textPtr)!;
        
        var msg     = formatter!.GetMessage(textPtr, args);

        if (Message == null) {
            var level = (TraceLogLevel) logLevel;
            switch (level) {
                case TraceLogLevel.All:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine($"[ALL] {msg}");
                    Console.ResetColor();
                    break;

                case TraceLogLevel.Trace:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"[TRACE] {msg}");
                    Console.ResetColor();
                    break;

                case TraceLogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"[DEBUG] {msg}");
                    Console.ResetColor();
                    break;

                case TraceLogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"[INFO] {msg}");
                    Console.ResetColor();
                    break;

                case TraceLogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"[WARNING] {msg}");
                    Console.ResetColor();
                    break;

                case TraceLogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[ERROR] {msg}");
                    Console.ResetColor();
                    break;

                case TraceLogLevel.Fatal:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[FATAL] {msg}");
                    Console.ResetColor();
                    break;

                default:
                    Console.WriteLine(textStr);
                    break;
            }

            return;
        }

        Message.Invoke((TraceLogLevel) logLevel, msg);
    }

    /// <summary>
    /// Destroys the logger (sets the trace log callback to null).
    /// </summary>
    public static unsafe void Destroy() {
        Message = null;
        Raylib.SetTraceLogCallback(null);
    }
}