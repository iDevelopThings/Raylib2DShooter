using System.Runtime.CompilerServices;
using Arch.Core;

namespace RLShooter.Common.ArchExtensions;

public static class EntityExtensions {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComponentHandle<T> GetHandle<T>(this Entity entity) where T : struct
        => new(entity);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComponentHandle<T> GetHandle<T>(this EntityReference entity) where T : struct
        => new(entity);

}