using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using RLShooter.Gameplay.Components;

namespace RLShooter.Utils;

public static class EntityExtensions {

    public static bool TryGetEditorFlags(ref this Entity entity, out EditorFlags flags)
        => entity.TryGet(out flags);
    public static ref EditorFlags TryGetEditorFlagsRef(ref this Entity entity, out bool success)
        => ref entity.TryGetRef<EditorFlags>(out success);
    
    public static bool TryGetNamed(ref this Entity entity, out Named named)
        => entity.TryGet(out named);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetEntityName(ref this Entity entity)
        => entity.TryGet<Named>(out var named) ? named.Name : entity.ToString();
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSelectedInEditor(ref this Entity entity)
        => entity.TryGetEditorFlags(out var flags) && flags.IsSelectedInEditor;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetSelectedInEditor(ref this Entity entity, bool value) {
        ref var flags = ref entity.TryGetEditorFlagsRef(out var success);
        if (success) {
            flags.IsSelectedInEditor = value;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOpenInEditor(ref this Entity entity)
        => entity.TryGetEditorFlags(out var flags) && flags.OpenInEditor;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetOpenInEditor(ref this Entity entity, bool value) {
        ref var flags = ref entity.TryGetEditorFlagsRef(out var success);
        if (success) {
            flags.OpenInEditor = value;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDisplayedInEditor(ref this Entity entity)
        => entity.TryGetEditorFlags(out var flags) && flags.DisplayedInEditor;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetDisplayedInEditor(ref this Entity entity, bool value) {
        ref var flags = ref entity.TryGetEditorFlagsRef(out var success);
        if (success) {
            flags.DisplayedInEditor = value;
        }
    }

}