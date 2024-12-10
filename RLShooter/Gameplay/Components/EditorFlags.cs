using RLShooter.App.Editor.Windows.Inspectors;

namespace RLShooter.Gameplay.Components;

public struct EditorFlags {

    private enum InternalEditorFlags : byte {
        None = 0,

        /// <summary>
        /// Is selected in the scene hierarchy widget.
        /// </summary>
        Selected = 1 << 0,

        /// <summary>
        /// Is open in the scene hierarchy widget.
        /// </summary>
        Open = 1 << 1,

        /// <summary>
        /// Is displayed in the scene hierarchy widget.
        /// </summary>
        Displayed = 1 << 2,
    }

    private InternalEditorFlags _editorFlags = InternalEditorFlags.None;

    public EditorFlags() { }

    public static EditorFlags Default => new() {
        _editorFlags = InternalEditorFlags.Displayed,
    };

    [InspectorCategory("GameObject/States/EditorFlags")]
    public bool IsSelectedInEditor {
        get => (_editorFlags & InternalEditorFlags.Selected) != 0;
        set => _editorFlags = value ? _editorFlags | InternalEditorFlags.Selected : _editorFlags & ~InternalEditorFlags.Selected;
    }
    [InspectorCategory("GameObject/States/EditorFlags")]
    public bool OpenInEditor {
        get => (_editorFlags & InternalEditorFlags.Open) != 0;
        set => _editorFlags = value ? _editorFlags | InternalEditorFlags.Open : _editorFlags & ~InternalEditorFlags.Open;
    }
    [InspectorCategory("GameObject/States/EditorFlags")]
    public bool DisplayedInEditor {
        get => (_editorFlags & InternalEditorFlags.Displayed) != 0;
        set => _editorFlags = value ? _editorFlags | InternalEditorFlags.Displayed : _editorFlags & ~InternalEditorFlags.Displayed;
    }

}