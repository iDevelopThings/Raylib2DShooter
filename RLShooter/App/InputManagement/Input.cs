using System.Runtime.CompilerServices;
using Hexa.NET.ImGui;

namespace RLShooter.App.InputManagement;

[Flags]
public enum InputState {
    None     = 0,
    Started  = 1 << 0,
    Pressed  = 1 << 1,
    Held     = 1 << 2,
    Released = 1 << 3,
}

public enum InputValueType {
    None,
    Boolean,
    Scalar,
    Vector2,
    Vector3,
}

public static class Input {
    private static Dictionary<string, InputAction> _actions = new();

    private static bool _isInputBlocked;
    public static bool IsInputBlocked {
        get {
            if (_isInputBlocked)
                return true;

            var io = ImGui.GetIO();

            return io.WantCaptureMouse || io.WantCaptureKeyboard;
        }

        set => _isInputBlocked = value;
    }

    public static InputAction RegisterAction(string name) {
        var action = new InputAction {
            Name = name,
        };
        return RegisterAction(name, action);
    }
    public static InputAction RegisterAction(string name, InputAction action) {
        _actions.TryAdd(name, action);

        return action;
    }

    public static void UnregisterAction(string name) {
        _actions.Remove(name);
    }

    public static bool IsActionInState(string name, InputTriggerState state) {
        if (IsInputBlocked)
            return false;

        return _actions.TryGetValue(name, out var action)
            && action.IsInState(state);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKeyDown(KeyboardKey key) => !IsInputBlocked && Raylib.IsKeyDown((int) key);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKeyPressed(KeyboardKey key) => !IsInputBlocked && Raylib.IsKeyPressed((int) key);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKeyUp(KeyboardKey key) => !IsInputBlocked && Raylib.IsKeyUp((int) key);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKeyReleased(KeyboardKey key) => !IsInputBlocked && Raylib.IsKeyReleased((int) key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMouseButtonDown(MouseButton key) => !IsInputBlocked && Raylib.IsMouseButtonDown((int) key);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMouseButtonPressed(MouseButton key) => !IsInputBlocked && Raylib.IsMouseButtonPressed((int) key);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMouseButtonUp(MouseButton key) => !IsInputBlocked && Raylib.IsMouseButtonUp((int) key);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMouseButtonReleased(MouseButton key) => !IsInputBlocked && Raylib.IsMouseButtonReleased((int) key);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetMouseWheel() => !IsInputBlocked ? GetMouseWheelMove() : 0;

    public static void Update() {
        if (IsInputBlocked)
            return;

        foreach (var action in _actions.Values) {
            action.Update();
        }
    }

    public static void RemapAction(string name, List<KeyBinding> newBindings) {
        if (_actions.TryGetValue(name, out var action)) {
            action.Bindings = newBindings;
        }
    }
}