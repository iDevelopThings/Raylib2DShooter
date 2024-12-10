using System.Runtime.CompilerServices;
using RLShooter.GameScene;
#pragma warning disable CS0414 // Field is assigned but its value is never used

namespace RLShooter.App.InputManagement;

[Flags]
public enum InputTriggerState {
    None     = 0,
    Started  = 1 << 0,
    Pressed  = 1 << 1,
    Held     = 1 << 2,
    Released = 1 << 3,

    TriggeredByKey     = 1 << 4,
    TriggeredByTrigger = 1 << 5,

    PressedOrHeld = Pressed | Held,
}

public struct InputActionValue {
    private InputValueType _containingType;
    private bool           _boolValue;
    private float          _scalarValue;
    private Vector2        _vector2Value;
    private Vector3        _vector3Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValue<T>() {
        // Use containing type to determine what to return
        return typeof(T) switch {
            { } t when t == typeof(bool)    => (T) (object) _boolValue,
            { } t when t == typeof(float)   => (T) (object) _scalarValue,
            { } t when t == typeof(int)     => (T) (object) (int) _scalarValue,
            { } t when t == typeof(double)  => (T) (object) (double) _scalarValue,
            { } t when t == typeof(Vector2) => (T) (object) _vector2Value,
            { } t when t == typeof(Vector3) => (T) (object) _vector3Value,
            _                               => default,
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public InputActionValue() {
        _containingType = InputValueType.None;
        _boolValue      = false;
        _scalarValue    = 0;
        _vector2Value   = Vector2.Zero;
        _vector3Value   = Vector3.Zero;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public InputActionValue(bool value) : this() {
        _containingType = InputValueType.Boolean;
        _boolValue      = value;
        _scalarValue    = value ? 1 : 0;
        _vector2Value   = value ? Vector2.One : Vector2.Zero;
        _vector3Value   = value ? Vector3.One : Vector3.Zero;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public InputActionValue(float value) : this() {
        _containingType = InputValueType.Scalar;
        _boolValue      = value != 0;
        _scalarValue    = value;
        _vector2Value   = new Vector2(value, value);
        _vector3Value   = new Vector3(value, value, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public InputActionValue(Vector2 value) : this() {
        _containingType = InputValueType.Vector2;
        _boolValue      = value != Vector2.Zero;
        _scalarValue    = value.X;
        _vector2Value   = value;
        _vector3Value   = new Vector3(value, 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public InputActionValue(Vector3 value) : this() {
        _containingType = InputValueType.Vector3;
        _boolValue      = value != Vector3.Zero;
        _scalarValue    = value.X;
        _vector2Value   = new Vector2(value.X, value.Y);
        _vector3Value   = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator InputActionValue(bool value) => new(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(InputActionValue value) => value._boolValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator InputActionValue(float value) => new(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator float(InputActionValue value) => value._scalarValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator InputActionValue(Vector2 value) => new(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector2(InputActionValue value) => value._vector2Value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator InputActionValue(Vector3 value) => new(value);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3(InputActionValue value) => value._vector3Value;

}

public class InputAction {

    public string Name { get; set; }

    public List<KeyBinding>     Bindings  { get; set; } = new();
    public List<IInputModifier> Modifiers { get; set; } = new();
    public List<IInputTrigger>  Triggers  { get; set; } = new();

    private InputTriggerState _lastTriggerState;
    private InputActionValue  _lastValue;

    // private bool              _wasPressedLastFrame;
    // private double            _lastPressedTime;
    // public bool   WasPressedLastFrame => _wasPressedLastFrame;
    // public double LastPressedTime     => _lastPressedTime;
    // public double PressedDuration     => Time.Since(_lastPressedTime);

    public event Action<InputAction> OnTriggered;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsInState(InputTriggerState state) => _lastTriggerState.HasFlag(state);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsTriggered() =>
        _lastTriggerState.HasFlag(InputTriggerState.TriggeredByKey) ||
        _lastTriggerState.HasFlag(InputTriggerState.TriggeredByTrigger);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPressed() => _lastTriggerState.HasFlag(InputTriggerState.Pressed);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsHeld() => _lastTriggerState.HasFlag(InputTriggerState.Held);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsReleased() => _lastTriggerState.HasFlag(InputTriggerState.Released);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsPressedOrHeld() => _lastTriggerState.HasFlag(InputTriggerState.PressedOrHeld);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValue<T>() => _lastValue.GetValue<T>();

    public InputAction AddOnTriggered(Action<InputAction> action) {
        OnTriggered += action;
        return this;
    }

    public InputAction AddBinding(KeyBinding binding) {
        Bindings.Add(binding);
        return this;
    }

    public InputAction AddModifier(IInputModifier modifier) {
        Modifiers.Add(modifier);
        return this;
    }

    public InputAction AddTrigger(IInputTrigger trigger) {
        Triggers.Add(trigger);
        return this;
    }

    public void Update() {
        // var wasPressed       = _wasPressedLastFrame;
        var value = new InputActionValue();

        _lastTriggerState = GetState(ref value);
        _lastValue        = value;

        if (_lastTriggerState != InputTriggerState.None) {
            OnTriggered?.Invoke(this);
        }

        // _wasPressedLastFrame = AreBindingsPressed();
        //
        // if (_wasPressedLastFrame && !wasPressed) {
        //     _lastPressedTime = Time.GetTotalTime();
        // }
    }

    public InputTriggerState GetState(ref InputActionValue value) {
        var state = InputTriggerState.None;

        foreach (var binding in Bindings) {
            if (!binding.CanExecute()) {
                continue;
            }
            
            if (!binding.IsPressed()) {
                return InputTriggerState.None;
            }

            state |= InputTriggerState.TriggeredByKey;
            value =  binding.GetValue();
        }

        foreach (var modifier in Modifiers) {
            if (!modifier.Apply(Bindings, ref value)) {
                return InputTriggerState.None;
            }
        }

        foreach (var trigger in Triggers) {
            var triggerState = trigger.Check(Bindings);
            if (triggerState != InputState.None) {
                state |= InputTriggerState.TriggeredByTrigger;
                return state;
            }

            return InputTriggerState.None;
        }

        return state;
    }

    /*public bool IsTriggered(InputState state) {
        var areBindingsPressed = AreBindingsPressed();
        switch (state) {
            case InputState.Pressed:
                return areBindingsPressed && !_wasPressedLastFrame;
            case InputState.Held:
                return areBindingsPressed;
            case InputState.Released:
                return !areBindingsPressed && _wasPressedLastFrame;
            default:
                return false;
        }
    }

    private bool AreBindingsPressed() {
        foreach (var binding in Bindings) {
            if (!binding.IsPressed()) {
                return false;
            }
        }
        return true;
    }*/
}