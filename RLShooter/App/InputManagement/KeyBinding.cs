using System.Runtime.CompilerServices;

namespace RLShooter.App.InputManagement;

public abstract class KeyBinding {
    public virtual bool CanExecute() => true;

    public abstract bool             IsPressed();
    public abstract InputActionValue GetValue();
}

public abstract class KeyBindingGroup<T> : KeyBinding where T : KeyBindingGroup<T> {
    public List<KeyBinding> Bindings { get; set; } = new();

    public bool RequireAllPressed { get; set; } = false;

    protected KeyBindingGroup() { }
    protected KeyBindingGroup(params KeyBinding[] bindings) {
        Bindings.AddRange(bindings);
    }
    protected KeyBindingGroup(bool requireAllPressed, params KeyBinding[] bindings)
        : this(bindings) {
        RequireAllPressed = requireAllPressed;
    }

    // we can only execute if we're not using a gamepad
    // public override bool CanExecute() =>
    // !IsGamepadAvailable(0);

    public T Add(params KeyBinding[] bindings) {
        Bindings.AddRange(bindings);

        return this as T;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsPressed() {
        if (RequireAllPressed) {
            foreach (var binding in Bindings) {
                if (!binding.IsPressed())
                    return false;
            }

            return true;
        }


        foreach (var binding in Bindings) {
            if (binding.IsPressed())
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override InputActionValue GetValue() => new(IsPressed());

}

public class MouseKeyboardGroup : KeyBindingGroup<MouseKeyboardGroup> {
    public MouseKeyboardGroup(params KeyBinding[] bindings)  : base(bindings) { }
    public MouseKeyboardGroup(bool requireAllPressed, params KeyBinding[] bindings): base(requireAllPressed, bindings) { }
    
    public override bool CanExecute() =>
        !IsGamepadAvailable(0);
}

public class GamepadGroup : KeyBindingGroup<GamepadGroup> {
    public GamepadGroup(params KeyBinding[] bindings)  : base(bindings) { }
    public GamepadGroup(bool                requireAllPressed, params KeyBinding[] bindings): base(requireAllPressed, bindings) { }

    public override bool CanExecute() =>
        IsGamepadAvailable(0);
}

public class KeyboardKeyBinding : KeyBinding {
    public KeyboardKey Key { get; }

    public KeyboardKeyBinding(KeyboardKey key) {
        Key = key;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsPressed()
        => IsKeyDown((int) Key);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override InputActionValue GetValue() => new(IsPressed());
}

public class MouseKeyBinding : KeyBinding {
    public MouseButton MouseButton { get; }

    public MouseKeyBinding(MouseButton mouseButton) {
        MouseButton = mouseButton;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsPressed()
        => IsMouseButtonDown((int) MouseButton);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override InputActionValue GetValue() => new(IsPressed());
}

public class GamepadButtonBinding : KeyBinding {
    public int           Gamepad { get; }
    public GamepadButton Button  { get; }

    public GamepadButtonBinding(int gamepad, GamepadButton button) {
        Gamepad = gamepad;
        Button  = button;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsPressed()
        => IsGamepadButtonDown(Gamepad, (int) Button);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override InputActionValue GetValue() => new(IsPressed());
}

public class AxisBinding : KeyBinding {
    public KeyBinding PositiveKey { get; }
    public KeyBinding NegativeKey { get; }

    public AxisBinding(KeyBinding positiveKey, KeyBinding negativeKey) {
        PositiveKey = positiveKey;
        NegativeKey = negativeKey;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool IsPressed() => PositiveKey.IsPressed() || NegativeKey.IsPressed();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override InputActionValue GetValue() {
        float value = 0;

        if (PositiveKey.IsPressed())
            value += 1;
        if (NegativeKey.IsPressed())
            value -= 1;

        return value;

    }
}