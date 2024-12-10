using Arch.Core;
using Arch.Core.Extensions;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace RLShooter.Common.ArchExtensions;

public class ComponentHandle<T> where T : struct {
    private readonly EntityReference _entity;

    public Entity Entity  => _entity;
    public bool   IsAlive => _entity.IsAlive();

    public ComponentHandle(Entity entity) {
        _entity = entity.Reference();
    }
    public ComponentHandle(EntityReference entity) {
        _entity = entity;
    }

    public T Value {
        get => _entity.Entity.Get<T>();
        set => _entity.Entity.Set(value);
    }

    /*public unsafe ref T ValueRef {
        get {
            fixed (T* ptr = &_entity.Entity.Get<T>()) {
                return ref *ptr;
            }
        }
    }*/

    public unsafe ref T Ref() {
        fixed (T* ptr = &_entity.Entity.Get<T>()) {
            return ref *ptr;
        }
    }

    public static implicit operator T(ComponentHandle<T> handle) => handle.Value;
}