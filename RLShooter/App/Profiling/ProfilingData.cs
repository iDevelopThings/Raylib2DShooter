using System.Collections.Concurrent;
using System.Diagnostics;
using ValueVariant;

namespace RLShooter.App.Profiling;

public class ProfilingData(string section) {
    public string                  Name       { get; set; } = section;
    public Stopwatch               Stopwatch  { get; set; } = new();
    public double                  TotalTime  { get; set; } = 0;
    public int                     FrameCount { get; set; } = 0;
    public double                  MinTime    { get; set; } = double.MaxValue;
    public double                  MaxTime    { get; set; } = double.MinValue;
    public ConcurrentQueue<double> FrameTimes { get; set; } = new();
    public double                  LastTime   { get; set; }

    public double AverageTime => TotalTime / FrameCount;

    public double[] GetFrameTimes() {
        return FrameTimes.ToArray();
    }
}

public class ProfilingValueData(ProfilerValueGroup group, string section) {
    public ProfilerValueGroup Group { get; set; } = group;
    public string             Name  { get; set; } = section;

    public Func<string> GetValue { get; set; }

    public string TitleString => $"{Group} - {Name}";
}