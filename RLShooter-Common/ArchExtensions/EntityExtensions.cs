using System.Runtime.CompilerServices;
using fennecs;

namespace RLShooter.Common.ArchExtensions;

public static class EntityExtensions {

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ComponentHandle<T> GetHandle<T>(this Entity entity) where T : struct
        => new(entity);

}