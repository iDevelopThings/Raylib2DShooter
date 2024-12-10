using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using RLShooter.Config;

namespace RLShooter.App.Profiling;

using System;
using System.Collections.Generic;

public partial class ProfilerConfig : ConfigBase<ProfilerConfig> {
    [ConfigValue, DefaultValue(false)]
    private bool _enabled = false;

    [ConfigValue]
    private ObservableCollection<string> _enabledBlocks;

    [ConfigValue, DefaultValue(1000)]
    private int _samplesPerSecond = 1000;

    [ConfigValue, DefaultValue(true)]
    private bool _sampleMemory = true;
    [ConfigValue, DefaultValue(50)]
    private int _samplesPerSecondMem = 50;

    [ConfigValue, DefaultValue(true)]
    private bool _sortByLargestTime = true;

    [OnDeserialized]
    public void OnDeserializedMethod(StreamingContext context) {
        if (_enabledBlocks == null) {
            _enabledBlocks = new();
        }
        _enabledBlocks.CollectionChanged += (sender, args) => Save();
    }
}

public enum ProfilerValueGroup {
    Transforms,
    Scene,
}

public class Profiler {
    private static Profiler _instance;

    private ConcurrentDictionary<string, ProfilingData> _timers = new();

    private ConcurrentDictionary<KeyValuePair<string, ProfilerValueGroup>, ProfilingValueData> _values = new();

    public static int MaxSamplePoints = 100;

    public static Profiler Instance => _instance ??= new Profiler();

    public void Start(string section) {
        if (!ProfilerConfig.Enabled)
            return;

        if (!_timers.TryGetValue(section, out var value)) {
            value            = new ProfilingData(section);
            _timers[section] = value;
        }

        value.Stopwatch.Restart();
    }

    public void Stop(string section) {
        if (!ProfilerConfig.Enabled)
            return;

        if (!_timers.TryGetValue(section, out var value))
            return;

        value.Stopwatch.Stop();
        var elapsedMs = value.Stopwatch.Elapsed.TotalMilliseconds;

        value.TotalTime += elapsedMs;
        value.FrameCount++;

        if (elapsedMs < value.MinTime)
            value.MinTime = elapsedMs;
        if (elapsedMs > value.MaxTime)
            value.MaxTime = elapsedMs;

        value.LastTime = elapsedMs;

        value.FrameTimes.Enqueue(elapsedMs);
        if (value.FrameTimes.Count > MaxSamplePoints) {
            value.FrameTimes.TryDequeue(out var _);
        }
    }

    public void Value(ProfilerValueGroup group, string section, Func<string> getter) {
        if (!ProfilerConfig.Enabled)
            return;

        var key = new KeyValuePair<string, ProfilerValueGroup>(section, group);
        if (!_values.TryGetValue(key, out var data)) {
            data          = new ProfilingValueData(group, section);
            data.GetValue = getter;
            
            _values[key]  = data;
        }
        
    }

    public double[] GetFrameTimes(string section) {
        return _timers.TryGetValue(section, out var value)
            ? value.FrameTimes.ToArray()
            : [];

    }

    public IEnumerable<ProfilingData> GetSections() {
        return _timers.Values;
    }
    public IEnumerable<ProfilingValueData> GetValues() {
        return _values.Values;
    }
}