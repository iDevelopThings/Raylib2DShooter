using RLShooter.Common.Utils;
using RLShooter.Config;

namespace RLShooter.App;

public class Window {
    public Vector2 WindowSize {
        get => AppConfig.WindowSize;
        set => AppConfig.WindowSize = value;
    }

    public Vector2 WindowPosition {
        get => AppConfig.WindowPosition;
        set => AppConfig.WindowPosition = value;
    }

    public bool IsWindowFocused { get; private set; }
    public bool HasMouseCapture { get; set; }

    public static Viewport Viewport { get; set; }

    public Window(bool createWindow = true) {
        if (createWindow) {
            SetConfigFlags(
                (int) (
                    ConfigFlags.FlagMsaa4XHint
                  | ConfigFlags.FlagWindowResizable
                  | ConfigFlags.FlagWindowHighdpi
                  | ConfigFlags.FlagWindowUnfocused
                  | ConfigFlags.FlagVsyncHint
                )
            );

            InitWindow((int) WindowSize.X, (int) WindowSize.Y, "Game");
            SetTargetFPS(0);
            SetWindowPosition((int) WindowPosition.X, (int) WindowPosition.Y);
            SetExitKey((int)KeyboardKey.F4);
        }

        Viewport = new(WindowPosition, WindowSize);

        ResizeViewport();
    }

    private void ResizeViewport() {
        Viewport = new(WindowPosition, WindowSize);
        // CoreEvents.OnViewportResized?.Invoke(Viewport);
    }

    public void Tick() {
        if (!IsWindowReady())
            return;

        if (IsWindowResized()) {
            WindowSize = new Vector2(GetRenderWidth(), GetRenderHeight());
            ResizeViewport();
        }

        var windowPos = GetWindowPosition();
        if (windowPos != WindowPosition) {
            WindowPosition = windowPos;
            ResizeViewport();
        }

        var isFocused = IsWindowFocused() && IsCursorOnScreen();
        if (isFocused != IsWindowFocused) {
            IsWindowFocused = isFocused;

            if (IsWindowFocused) {
                // HasMouseCapture = true;
            } else {
                HasMouseCapture = false;
                // EnableCursor();
            }
        }

        if (HasMouseCapture && IsKeyPressed((int)KeyboardKey.Escape)) {
            HasMouseCapture = false;
            // EnableCursor();
        }

        // if we have window focus, and we left click, we want to capture the mouse
        if (IsWindowFocused && (IsMouseButtonPressed((int)MouseButton.Left) || IsMouseButtonPressed((int)MouseButton.Right))) {
            HasMouseCapture = true;
            // SetMousePosition(GetScreenWidth() / 2, GetScreenHeight() / 2);
            // DisableCursor();
        }

        if (HasMouseCapture && IsKeyPressed((int)KeyboardKey.F1)) {
            AppConfig.IsEditorUiEnabled = !AppConfig.IsEditorUiEnabled;
            Console.WriteLine($"Editor ui {(AppConfig.IsEditorUiEnabled ? "enabled" : "disabled")}");
        }

    }
    public void Close() {
        CloseWindow();
    }
}