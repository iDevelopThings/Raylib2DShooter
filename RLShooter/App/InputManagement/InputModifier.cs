namespace RLShooter.App.InputManagement;

public class BaseInputModifierTrigger {
    public InputAction Action { get; set; }
}

public interface IInputModifier {
    public InputAction Action { get; set; }

    bool Apply(List<KeyBinding> bindings, ref InputActionValue value);
}

public interface IInputTrigger {
    public InputAction Action { get; set; }

    InputState Check(List<KeyBinding> bindings);
}

public class DeadZoneModifier : BaseInputModifierTrigger, IInputModifier {
    private float _deadZone;

    public DeadZoneModifier(float deadZone) {
        _deadZone = deadZone;
    }

    public bool Apply(List<KeyBinding> bindings, ref InputActionValue value) {
        foreach (var binding in bindings) {
            if (binding is AxisBinding axisBinding) {
                if (Math.Abs(axisBinding.GetValue()) < _deadZone) {
                    return false;
                }
            }
        }
        return true;
    }
}

public class PressTrigger : BaseInputModifierTrigger, IInputTrigger {

    public PressTrigger() { }

    private bool _wasPressed;

    public InputState Check(List<KeyBinding> bindings) {
        var state = InputState.None;

        foreach (var binding in bindings) {
            if (binding.IsPressed()) {
                state |= InputState.Started;
                state |= InputState.Pressed;

                _wasPressed = true;

                return state;
            }

        }

        if (_wasPressed) {
            state       |= InputState.Released;
            _wasPressed =  false;
        }

        return state;
    }
}

public class HoldTrigger : BaseInputModifierTrigger, IInputTrigger {
    private float                         _requiredHoldTime;
    private Dictionary<KeyBinding, float> _holdTimes = new();

    public HoldTrigger(float requiredHoldTime) {
        _requiredHoldTime = requiredHoldTime;
    }

    public InputState Check(List<KeyBinding> bindings) {
        var state = InputState.None;
        foreach (var binding in bindings) {
            if (_holdTimes.TryAdd(binding, 0)) {
                state |= InputState.Started;
            }

            if (binding.IsPressed()) {
                _holdTimes[binding] += GetFrameTime();
                if (_holdTimes[binding] >= _requiredHoldTime) {
                    state |= InputState.Held;
                    return state;
                }
            } else {
                _holdTimes[binding] = 0;

                if (state.HasFlag(InputState.Held)) {
                    state |= InputState.Released;
                }
            }
        }

        return state;
    }
}