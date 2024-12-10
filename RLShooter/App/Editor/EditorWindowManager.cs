using System.ComponentModel;
using Arch.Core;
using Hexa.NET.ImGui;
using RLShooter.App.Editor.ImGuiIntegration;
using RLShooter.App.Profiling;
using RLShooter.Config;
using RLShooter.GameScene;
using RLShooter.Common.Utils.Reflection;
using ImGui = Hexa.NET.ImGui.ImGui;

namespace RLShooter.App.Editor;

[ContainerSingleton]
public class EditorWindowManager : Singleton<EditorWindowManager>, IDisposable {
    private bool ranWindowInitialization = false;

    public ImGuiManager imGui { get; set; }

    public bool ImGuiDemoOpen;

    public Dictionary<Type, EditorWindow> Windows = new();

    public List<EditorWindow> FocusedWindows = new();

    public EditorWindowManager() { }
    private void OnAppConfigChanged(object sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(AppConfig.IsEditorUiEnabled)) {
            if (AppConfig.IsEditorUiEnabled) {
                Initialize();
            } else {
                Unload();
            }
        }
    }

    public void Initialize() {
        AppConfig.Instance.PropertyChanged += OnAppConfigChanged;

        if (!AppConfig.IsEditorUiEnabled)
            return;

        imGui = new ImGuiManager();

        var io = ImGui.GetIO();
        io.ConfigWindowsMoveFromTitleBarOnly = true;

        InitializeAllEditorWindows();
    }

    private void InitializeAllEditorWindows() {
        if (!AppConfig.IsEditorUiEnabled)
            return;

        var windowTypes = ReflectionStore.AllTypesOf<EditorWindow>().ToList();
        foreach (var windowType in windowTypes) {
            if (windowType.IsAbstract)
                continue;

            AppContainer.RegisterSingleton(windowType);

            var w = AppContainer.Resolve<EditorWindow>(windowType);
            w.OnWindowFocused += () => {
                if (!FocusedWindows.Contains(w))
                    FocusedWindows.Add(w);
            };
            w.OnWindowUnfocused += () => {
                FocusedWindows.Remove(w);

            };

            Windows.Add(windowType, w);
        }
    }

    public void RenderFrame() {
        if (!AppConfig.IsEditorUiEnabled)
            return;
        if (!ranWindowInitialization)
            return;

        DrawMenuBar();
        Draw();

    }

    private void DrawMenuBar() {
        if (ImGui.BeginMainMenuBar()) {
            if (ImGui.BeginMenu("Window")) {
                if (ImGui.MenuItem("Exit")) {
                    Application.ShouldQuit = true;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Window")) {
                ImGui.MenuItem("ImGui Demo", string.Empty, ref ImGuiDemoOpen);

                foreach (var (_, window) in Windows) {
                    var pOpen = window.Open;
                    if (ImGui.MenuItem(window.Title, string.Empty, ref pOpen)) {
                        window.Open = pOpen;
                    }
                }

                ImGui.EndMenu();
            }


            var isPlaying     = Application.InPlayMode;
            var availableSize = ImGui.GetContentRegionAvail();
            if (!isPlaying) {

                var available = (availableSize.X / 2) - ((80 + 100) / 2);
                ImGui.SetCursorPosX(available);

                if (ImGui.Button($"{UwU.Play} Play", new Vector2(80, 30))) {
                    Application.Play(Application.CurrentScene);
                }

            } else {

                var available = (availableSize.X / 2) - ((80) / 2);
                ImGui.SetCursorPosX(available);

                if (ImGui.Button($"{UwU.Stop} Stop", new Vector2(80, 30))) {
                    Application.StopPlaying();
                }
            }

            ImGui.SameLine();

            var fpsText     = $"FPS: {Time.GetFPS()}";
            var fpsTextSize = ImGui.CalcTextSize(fpsText);

            ImGui.SetCursorPosX(availableSize.X - (fpsTextSize.X / 4));
            ImGui.Text(fpsText);

            var profilingText = $"{UwU.Gear} Profiling {(ProfilerConfig.Enabled ? "Enabled" : "Disabled")}";

            ImGui.SetCursorPosX(availableSize.X - 150 - fpsTextSize.X - 5);
            if (ImGui.Button(profilingText, new Vector2(150, 30))) {
                ProfilerConfig.Enabled = !ProfilerConfig.Enabled;
            }

            ImGui.EndMainMenuBar();
        }

        if (ImGuiDemoOpen)
            ImGui.ShowDemoWindow(ref ImGuiDemoOpen);
    }

    public void Update(float dt) {
        if (!AppConfig.IsEditorUiEnabled)
            return;

        if (!ranWindowInitialization) {
            ranWindowInitialization = true;
            foreach (var window in Windows.Values) {
                window.Setup(window.Open);
            }
        }

        foreach (var window in Windows.Values) {
            if (window.Open) {
                window.Update();
            }
        }
    }

    public void Draw() {
        if (!AppConfig.IsEditorUiEnabled)
            return;

        foreach (var window in Windows.Values) {
            using (new ProfilingScope($"Window Draw: {window.Title}")) {
                window.Draw();
            }
        }
    }

    public void Unload() {
        if (!AppConfig.IsEditorUiEnabled)
            return;

        foreach (var window in Windows.Values) {
            window.Shutdown();
        }

        Windows.Clear();
        FocusedWindows.Clear();

        imGui?.Dispose();

        ranWindowInitialization = false;

        AppConfig.Instance.PropertyChanged -= OnAppConfigChanged;
    }

    private void SetImGuiTheme() {
        var style = ImGui.GetStyle();

        style.Colors[(int) ImGuiCol.WindowBg] = new Vector4(0.1f, 0.105f, 0.11f, 1.0f);

        // Headers
        style.Colors[(int) ImGuiCol.Header]        = new Vector4(0.2f, 0.205f, 0.21f, 1.0f);
        style.Colors[(int) ImGuiCol.HeaderHovered] = new Vector4(0.3f, 0.305f, 0.31f, 1.0f);
        style.Colors[(int) ImGuiCol.HeaderActive]  = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);

        // Buttons
        style.Colors[(int) ImGuiCol.Button]        = new Vector4(0.2f, 0.205f, 0.21f, 1.0f);
        style.Colors[(int) ImGuiCol.ButtonHovered] = new Vector4(0.3f, 0.305f, 0.31f, 1.0f);
        style.Colors[(int) ImGuiCol.ButtonActive]  = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);

        // Frame BG
        style.Colors[(int) ImGuiCol.FrameBg]        = new Vector4(0.2f, 0.205f, 0.21f, 1.0f);
        style.Colors[(int) ImGuiCol.FrameBgHovered] = new Vector4(0.3f, 0.305f, 0.31f, 1.0f);
        style.Colors[(int) ImGuiCol.FrameBgActive]  = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);

        // Tabs
        style.Colors[(int) ImGuiCol.Tab]         = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);
        style.Colors[(int) ImGuiCol.TabHovered]  = new Vector4(0.38f, 0.3805f, 0.381f, 1.0f);
        style.Colors[(int) ImGuiCol.TabSelected] = new Vector4(0.28f, 0.2805f, 0.281f, 1.0f);


        // Title
        style.Colors[(int) ImGuiCol.TitleBg]          = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);
        style.Colors[(int) ImGuiCol.TitleBgActive]    = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);
        style.Colors[(int) ImGuiCol.TitleBgCollapsed] = new Vector4(0.15f, 0.1505f, 0.151f, 1.0f);
    }

    public void Dispose() {
        Unload();
    }
}