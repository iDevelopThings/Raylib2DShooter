using Hexa.NET.ImGui;

namespace RLShooter.App.Editor.ImGuiIntegration;

public struct ImguiStyleVarScope : IDisposable {
    bool _enabled;
    public ImguiStyleVarScope(ImGuiStyleVar styleVar, float value) {
        ImGui.PushStyleVar(styleVar, value);
        _enabled = true;
    }
    public ImguiStyleVarScope(bool ifEnabled, ImGuiStyleVar styleVar, float value) {
        _enabled = ifEnabled;
        if (ifEnabled) {
            ImGui.PushStyleVar(styleVar, value);
        }
    }
    public ImguiStyleVarScope(ImGuiStyleVar styleVar, Vector2 value) {
        ImGui.PushStyleVar(styleVar, value);
        _enabled = true;
    }
    public ImguiStyleVarScope(bool ifEnabled, ImGuiStyleVar styleVar, Vector2 value) {
        _enabled = ifEnabled;
        if (ifEnabled) {
            ImGui.PushStyleVar(styleVar, value);
        }
    }
    public void Dispose() {
        if (_enabled)
            ImGui.PopStyleVar();
    }
}

public struct ImguiDisabledScope : IDisposable {
    bool _disabled;
    public ImguiDisabledScope(bool disabled) {
        if (disabled) {
            ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
            _disabled = true;
        } else {
            _disabled = false;
        }
    }
    public void Dispose() {
        if (_disabled)
            ImGui.PopStyleVar();
    }
}

public struct ImguiItemDisabledScope : IDisposable {
    bool _disabled;
    public ImguiItemDisabledScope(bool disabled) {
        if (disabled) {
            ImGui.BeginDisabled();
            _disabled = true;
        } else {
            _disabled = false;
        }
    }
    public void Dispose() {
        if (_disabled)
            ImGui.EndDisabled();
    }
}

public struct ImguiIdScope : IDisposable {
    public ImguiIdScope(string id) {
        ImGui.PushID(id);
    }
    public void Dispose() {
        ImGui.PopID();
    }
}

public struct ImguiIndentScope : IDisposable {
    private bool _indent;
    public ImguiIndentScope(bool indent) {
        if (indent) {
            ImGui.Indent();
            _indent = true;
        } else {
            _indent = false;
        }
    }
    public void Dispose() {
        if (_indent)
            ImGui.Unindent();
    }
}