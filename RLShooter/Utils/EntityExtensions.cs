using System.Runtime.CompilerServices;
using fennecs;
using RLShooter.Gameplay.Components;

namespace RLShooter.Utils;

public static class EntityExtensions {

    public static bool TryGetComponent<T>(this Entity entity, out T component) where T : struct {
        if (entity.Has<T>()) {
            component = entity.Get<T>(Match.Any).First();
            return true;
        }
        component = default;
        return false;
    }
    public static ref T TryGetComponentRef<T>(this Entity entity) where T : struct {
        ref var comp = ref entity.Ref<T>(Match.Any);
        return ref comp;
    }

    public static bool TryGetEditorFlags(this Entity entity, out EditorFlags flags) {
        if (entity.Has<EditorFlags>()) {
            flags = entity.Get<EditorFlags>(Match.Any).First();
            return true;
        }
        flags = default;
        return false;
    }

    public static bool TryGetNamed(this Entity entity, out Named named)
        => entity.TryGetComponent(out named);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetEntityName(this Entity entity)
        => entity.TryGetComponent<Named>(out var named) ? named.Name : entity.ToString();


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSelectedInEditor(this Entity entity)
        => entity.TryGetEditorFlags(out var flags) && flags.IsSelectedInEditor;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetSelectedInEditor(this Entity entity, bool value) {
        ref var flags = ref entity.TryGetComponentRef<EditorFlags>();
        flags.IsSelectedInEditor = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsOpenInEditor(this Entity entity)
        => entity.TryGetEditorFlags(out var flags) && flags.OpenInEditor;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetOpenInEditor(this Entity entity, bool value) {
        ref var flags = ref entity.TryGetComponentRef<EditorFlags>();
        flags.OpenInEditor = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDisplayedInEditor(this Entity entity)
        => entity.TryGetEditorFlags(out var flags) && flags.DisplayedInEditor;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetDisplayedInEditor(this Entity entity, bool value) {
        ref var flags = ref entity.TryGetComponentRef<EditorFlags>();
        flags.DisplayedInEditor = value;
    }

}