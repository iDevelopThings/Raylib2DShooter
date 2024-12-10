using System.Collections.ObjectModel;
using System.Diagnostics;
using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using RLShooter.App.Profiling;
using RLShooter.Common.Utils;
using RLShooter.GameScene;

namespace RLShooter.App.Editor.Windows;

public class ProfilingWindow : EditorWindow {
    private float accum;
    private float accumMem;

    public int SamplesPerSecond {
        get => ProfilerConfig.SamplesPerSecond;
        set => ProfilerConfig.SamplesPerSecond = value;
    }
    public int SamplesPerSecondMem {
        get => ProfilerConfig.SamplesPerSecondMem;
        set => ProfilerConfig.SamplesPerSecondMem = value;
    }
    public bool SampleMemory {
        get => ProfilerConfig.SampleMemory;
        set => ProfilerConfig.SampleMemory = value;
    }
    public bool SortByLargestTime {
        get => ProfilerConfig.SortByLargestTime;
        set => ProfilerConfig.SortByLargestTime = value;
    }
    public ObservableCollection<string> EnabledProfilerBlocks {
        get => ProfilerConfig.EnabledBlocks;
        set => ProfilerConfig.EnabledBlocks = value;
    }

    public float SampleLatency    => 1f / SamplesPerSecond;
    public float MemSampleLatency => 1f / SamplesPerSecondMem;

    private Dictionary<ProfilingData, RingBuffer<double>> _blocks = new();

    public RingBuffer<double> MemoryUsage = new(ProfilerConfig.SamplesPerSecondMem) {AverageValues = false};


    public override void Setup(bool open) {
        base.Setup(open);
    }

    protected override void OnDraw() {
        if (!ProfilerConfig.Enabled) {
            ImGui.Text("Profiler is disabled");
            ImGui.Separator();
            if (ImGui.Button("Enable Profiler")) {
                ProfilerConfig.Enabled = true;
            }

            return;
        }

        SampleSections();

        ImGui.BeginTabBar("Profiler", ImGuiTabBarFlags.None);

        if (ImGui.BeginTabItem("Settings")) {
            DrawSettings();
            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem("Profiler Blocks")) {
            DrawValues();
            ImGui.Separator();
            DrawProfilerBlocks();
            ImGui.EndTabItem();
        }

        if (SampleMemory && ImGui.BeginTabItem("Memory")) {
            DrawMemory();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();

    }
    private void DrawValues() {

        Profiler.Instance.GetValues().GroupBy(k => k.Group)
           .ToList()
           .ForEach(valGroup => {
                ImGui.SeparatorText(valGroup.Key.ToString());
                foreach (var value in valGroup) {
                    ImGui.Text($"{value.Name}: {value.GetValue()}");
                }
            });

    }
    private void DrawSettings() {
        if (ImGui.Button("Disable Profiler")) {
            ProfilerConfig.Enabled = false;
        }

        bool sort = SortByLargestTime;
        if (ImGui.Checkbox("Sort by largest time", ref sort)) {
            SortByLargestTime = sort;
        }

        bool sampleMem = SampleMemory;
        if (ImGui.Checkbox("Sample Memory?", ref sampleMem)) {
            SampleMemory = sampleMem;
        }

        ImGui.Separator();

        var sections = Profiler.Instance.GetSections();
        if (SortByLargestTime) {
            sections = sections.OrderByDescending(s => s.AverageTime).ToList();
        }

        foreach (var section in sections) {
            if (section.FrameCount <= 0)
                continue;

            var enabled = !CanUseBlock(section.Name);
            ImGui.PushID(section.Name + "Checkbox");
            if (ImGui.Checkbox($"Disabled?", ref enabled)) {
                if (enabled) {
                    EnabledProfilerBlocks.Add(section.Name);
                } else {
                    EnabledProfilerBlocks.Remove(section.Name);
                }
            }

            ImGui.PopID();

            ImGui.SameLine();
            ImGui.SeparatorText(section.Name);

            ImGui.Text($"Average Time:");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1), $"{(section.TotalTime / section.FrameCount).FormatMsTime()}");

            ImGui.SameLine();

            ImGui.Text($"Min/Max Time:");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1), $"{section.MinTime.FormatMsTime()}");
            ImGui.SameLine();
            ImGui.Text("/");
            ImGui.SameLine();
            ImGui.TextColored(new Vector4(0.5f, 0.5f, 0.5f, 1), $"{section.MaxTime.FormatMsTime()}");

            ImGui.Separator();

        }


    }
    private void DrawMemory() {
        int memSamplesPerSecond = SamplesPerSecondMem;
        if (ImGui.SliderInt("Max Memory Sample Points", ref memSamplesPerSecond, 10, 5000)) {
            SamplesPerSecondMem = memSamplesPerSecond;
            MemoryUsage         = new RingBuffer<double>(SamplesPerSecondMem) {AverageValues = false};
        }

        ImGui.Separator();

        const int   shade_mode = 2;
        const float fill_ref   = 0;
        var fill = shade_mode switch {
            0 => -double.PositiveInfinity,
            1 => double.PositiveInfinity,
            _ => fill_ref,
        };

        if (MemoryUsage.Length == 0)
            return;


        ImPlot.SetNextAxesToFit();
        if (ImPlot.BeginPlot("Memory", new Vector2(-1, 0), ImPlotFlags.NoInputs)) {
            ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
            ImPlot.PlotShaded("Memory (RAM)", ref MemoryUsage.Values[0], MemoryUsage.Length, fill, 1, 0, ImPlotShadedFlags.None, MemoryUsage.Head);
            ImPlot.PopStyleVar();
            ImPlot.PlotLine("Memory (RAM)", ref MemoryUsage.Values[0], MemoryUsage.Length, 1, 0, ImPlotLineFlags.None, MemoryUsage.Head);
            ImPlot.EndPlot();
        }
    }

    private void DrawProfilerBlocks() {

        ImGui.Text($"FPS: {Time.GetFPS()}");
        ImGui.Text($"Latency: {Time.Delta}");

        int samplesPerSecond = SamplesPerSecond;
        if (ImGui.SliderInt("Max Sample Points", ref samplesPerSecond, 10, 5000)) {
            SamplesPerSecond = samplesPerSecond;
            foreach (var pair in _blocks) {
                _blocks[pair.Key] = new RingBuffer<double>(SamplesPerSecond) {AverageValues = false};
            }
        }

        ImGui.Separator();

        ImPlot.SetNextAxesToFit();

        /*var yMax = 0.0;
        foreach (var s in Profiler.Instance.GetSections()) {
            if (s.FrameCount <= 0)
                return;

            if (!CanUseBlock(s.Name))
                return;

            if (!_blocks.TryGetValue(s, out var block))
                return;

            yMax = Math.Max(yMax, block.Values.Max());
        }

        yMax += yMax * 2.0f;

        ImPlot.SetNextAxesLimits(
            0,
            SamplesPerSecond,
            0,
            yMax
        );*/

        var avail = ImGui.GetContentRegionAvail();
        // if (ImPlot.BeginPlot($"##Times", avail with {Y = 0})) {
        if (ImPlot.BeginPlot($"##Times", new Vector2(-1, 0), ImPlotFlags.NoInputs)) {
            foreach (var section in Profiler.Instance.GetSections()) {
                if (section.FrameCount <= 0)
                    continue;

                if (!CanUseBlock(section.Name))
                    continue;

                if (!_blocks.TryGetValue(section, out var block))
                    continue;


                ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
                ImPlot.PlotShaded(section.Name, ref block.Values[0], block.Length, 0, 1, 0, ImPlotShadedFlags.None, block.Head);
                ImPlot.PopStyleVar();

                // ImPlot.SetupAxes("Time", "ms", ImPlotAxisFlags.AutoFit);

                ImPlot.PlotLine(
                    section.Name,
                    ref block.Values[0],
                    block.Length,
                    1,
                    0,
                    ImPlotLineFlags.Loop,
                    block.Head
                );
            }

            ImPlot.EndPlot();
        }

    }

    private void SampleSections() {
        accum += Time.Delta;

        while (accum > SampleLatency) {
            Sample();
            accum -= SampleLatency;
        }

        if (SampleMemory) {
            accumMem += Time.Delta;
            while (accumMem > MemSampleLatency) {
                SampleMem();
                accumMem = 0;
            }
        }

    }

    public bool CanUseBlock(string name) => !EnabledProfilerBlocks.Contains(name);

    private void Sample() {
        foreach (var section in Profiler.Instance.GetSections()) {
            if (!CanUseBlock(section.Name))
                continue;

            if (!_blocks.TryGetValue(section, out var block)) {
                block            = new RingBuffer<double>(SamplesPerSecond) {AverageValues = false};
                _blocks[section] = block;
            }

            block.Add(section.LastTime);
        }
    }

    public void SampleMem() {
        MemoryUsage.Add(Process.GetCurrentProcess().PrivateMemorySize64 / 1000d / 1000d);
        // VideoMemoryUsage.Add(GraphicsAdapter.Current.GetMemoryCurrentUsage() / 1000d / 1000d);
    }
}