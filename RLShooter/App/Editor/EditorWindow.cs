using System.Numerics;
using Hexa.NET.ImGui;
using RLShooter.Config;

namespace RLShooter.App.Editor;

public abstract class EditorWindow {
    private bool _open;

    private bool    _previousHovered;
    private bool    _previousFocused;
    private Vector2 _previousSize;
    private Vector2 _previousPosition;

    public bool    Hovered;
    public bool    Focused;
    public Vector2 WindowSize;
    public Vector2 WindowPosition;

    public Action<Vector2, Vector2> OnWindowResized;
    public Action<Vector2, Vector2> OnWindowMoved;
    public Action                   OnWindowFocused;
    public Action                   OnWindowUnfocused;

    public bool Open {
        get => AppConfig.OpenEditorWindows.Contains(Identifier);
        set {
            if (value)
                AppConfig.OpenEditorWindows.Add(Identifier);
            else
                AppConfig.OpenEditorWindows.Remove(Identifier);

            AppConfig.SaveConfig();

            _open = value;
        }
    }

    public ImGuiWindowFlags WindowFlags = ImGuiWindowFlags.None;

    public string Identifier => GetType().Name;
    public string Title      => GetType().Name;

    public virtual void Setup(bool open) => Open = open;


    protected virtual bool CanDraw() => true;

    public void Draw() {
        if (!Open)
            return;

        if (!CanDraw())
            return;

        var io = ImGui.GetIO();

        OnPreDraw();

        var _windowOpen = _open;
        if (ImGui.Begin(Title, ref _windowOpen, WindowFlags)) {
            var currentSize = ImGui.GetWindowSize();
            var currentPos  = ImGui.GetWindowPos();

            var currentlyFocused = ImGui.IsWindowFocused(ImGuiFocusedFlags.DockHierarchy);
            var currentlyHovered = io.WantCaptureMouse;

            if (currentSize != _previousSize) {
                WindowSize = currentSize;
                OnWindowResized?.Invoke(_previousSize, currentSize);
            }

            if (currentPos != _previousPosition) {
                WindowPosition = currentPos;
                OnWindowMoved?.Invoke(_previousPosition, currentPos);
            }

            if (currentlyFocused != _previousFocused) {
                Focused = currentlyFocused;
                if (currentlyFocused) {
                    OnWindowFocused?.Invoke();
                } else {
                    OnWindowUnfocused?.Invoke();
                }
            }

            if (currentlyHovered != _previousHovered) {
                Hovered = currentlyHovered;
                if (currentlyHovered) {
                    OnWindowFocused?.Invoke();
                } else {
                    OnWindowUnfocused?.Invoke();
                }
            }

            _previousSize     = currentSize;
            _previousPosition = currentPos;
            _previousFocused  = currentlyFocused;
            _previousHovered  = currentlyHovered;

            ImGui.BeginChild("##content");
            OnDraw();
            ImGui.EndChild();
        }

        OnPostDraw();

        ImGui.End();

        if (_windowOpen != _open) {
            Open = _windowOpen;

            if (!_windowOpen) {
                OnClosed();
            }
        }
    }

    protected virtual void OnClosed() {
        Focused = false;
        Hovered = false;
        OnWindowUnfocused?.Invoke();
    }

    protected virtual void OnPreDraw() { }

    protected virtual void OnPostDraw() { }

    protected abstract void OnDraw();

    public virtual void Hide() {
        Open = false;
    }

    public virtual void Update() { }


    public virtual void Shutdown() { }
}