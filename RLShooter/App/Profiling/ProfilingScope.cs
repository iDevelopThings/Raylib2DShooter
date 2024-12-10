#pragma warning disable CS0169 // Field is never used

namespace RLShooter.App.Profiling;

public struct ProfilingScope : IDisposable {
    private string _section;
    public ProfilingScope(string section) {
#if PROFILE
        _section = section;
        Profiler.Instance.Start(_section);
#endif
    }

    public void Dispose() {
#if PROFILE
        Profiler.Instance.Stop(_section);
#endif
    }
}

public static class StopwatchExtensions {
    public static double TotalMicroseconds(this TimeSpan timeSpan) {
        return timeSpan.Ticks / (TimeSpan.TicksPerMillisecond / 1000.0);
    }
}