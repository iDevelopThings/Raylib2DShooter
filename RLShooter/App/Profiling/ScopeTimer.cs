using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Reflection;
using MethodDecorator.Fody.Interfaces;
using RLShooter.App.Logging;

namespace RLShooter.App.Profiling;

public struct ScopeTimer : IDisposable
{
    private static Logger _logger = new Logger("ScopeTimer");

    private readonly Stopwatch _stopwatch;
    private readonly string    _scopeName;

    public ScopeTimer([CallerMemberName] string scopeName = "") {
        _scopeName = scopeName;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Start() => _stopwatch.Restart();
    public void Stop() {
        _stopwatch.Stop();
        var elapsed = _stopwatch.Elapsed;

        string formattedTime;
        if (elapsed.TotalMilliseconds < 1) {
            formattedTime = $"{elapsed.TotalMicroseconds}us";
        } else if (elapsed.TotalSeconds < 1) {
            formattedTime = $"{elapsed.TotalMilliseconds:F1}ms";
        } else {
            formattedTime = $"{elapsed.TotalSeconds:F1}s";
        }

        // Console.WriteLine($"{_scopeName} -> {formattedTime}");
        _logger.Debug($"{_scopeName} -> {formattedTime}");
    }

    public void Dispose() => Stop();
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
public class TimedAttribute : Attribute
#if PROFILE
                            , IMethodDecorator
#endif
{
#if PROFILE

    private string     name;
    private ScopeTimer timer;

    public TimedAttribute(string name = null) {
        this.name = name;
    }

    public void Init(object instance, MethodBase method, object[] args) {
        if (name == null) {
            name = $"{method.DeclaringType!.Name}.{method.Name}";
            if(args.Length > 0 && args[0] != null) {
                // try to convert the first argument to a string
                name += $" -> {args[0]}";
            }
        }

        timer = new ScopeTimer(name);
    }

    public void OnEntry() => timer.Start();

    public void OnExit() => timer.Stop();

    public void OnException(Exception exception) { }

#endif
}