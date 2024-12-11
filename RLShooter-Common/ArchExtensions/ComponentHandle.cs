
using fennecs;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace RLShooter.Common.ArchExtensions;

public class ComponentHandle<T> where T : struct {
    private readonly Entity _entity;

    public Entity Entity  => _entity;
    public bool   IsAlive => _entity.Alive;

    public ComponentHandle(Entity entity) {
        _entity = entity;
    }

    public T Value {
        get => _entity.Get<T>(Match.Any).First();
        set => _entity.Set(value);
    }

    public unsafe ref T Ref() {
        fixed (T* ptr = &_entity.Ref<T>()) {
            return ref *ptr;
        }
    }

    public static implicit operator T(ComponentHandle<T> handle) => handle.Value;
}