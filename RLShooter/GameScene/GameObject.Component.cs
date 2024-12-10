using System.Numerics;
using RLShooter.App.Events;

namespace RLShooter.GameScene;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public class RequiredComponentAttribute : Attribute;

public partial class GameObject {

    [EventHandler]
    private readonly EventHandlers<GameObject, IComponent> componentAddedListeners   = new();
    [EventHandler]
    private readonly EventHandlers<GameObject, IComponent> componentRemovedListeners = new();

    // public event EventHandler<GameObject, IComponent> ComponentAdded {
    //     add => componentAddedListeners.AddHandler(value);
    //     remove => componentAddedListeners.RemoveHandler(value);
    // }
    //
    // public event EventHandler<GameObject, IComponent> ComponentRemoved {
    //     add => componentRemovedListeners.AddHandler(value);
    //     remove => componentRemovedListeners.RemoveHandler(value);
    // }

    public IComponent AddComponent(IComponent component, Action<IComponent> initFn = null) {

        Components.Add(component);
        component.Owner = this;

        if (!ComponentMap.ContainsKey(component.GetType())) {
            ComponentMap.Add(component.GetType(), []);
        }
        ComponentMap[component.GetType()].Add(component);

        initFn?.Invoke(component);

        componentAddedListeners.Invoke(this, component);

        component.OnAddedToGameObject();


        return component;
    }

    public IComponent AddComponent(Type componentType, Action<IComponent> initFn = null) {
        IComponent component = null;

        // If the component has a public constructor which takes a GameObject, use that
        if (componentType.GetConstructor(new[] {typeof(GameObject)}) is { } constructor) {
            component = (IComponent) constructor.Invoke(new object[] {this});
        } else {
            component = (IComponent) Activator.CreateInstance(componentType);
        }

        return AddComponent(component, initFn);
    }

    public T AddComponent<T>(Action<T> initFn = null) where T : class, IComponent, new()
        => AddComponent(typeof(T), component => {
            if (component is T t)
                initFn?.Invoke(t);
        }) as T;

    public void RemoveComponent(IComponent component) {
        if (component == null) {
            return;
        }

        if (!ComponentMap.ContainsKey(component.GetType())) {
            return;
        }
        ComponentMap[component.GetType()].Remove(component);
        Components.Remove(component);

        componentRemovedListeners.Invoke(this, component);

        component.OnRemovedFromGameObject();
    }

    public void RemoveComponent<T>() where T : class, IComponent {
        if (!ComponentMap.ContainsKey(typeof(T))) {
            return;
        }
        RemoveComponent(GetComponent<T>());
    }

    public T GetComponent<T>() where T : class, IComponent {
        if (!ComponentMap.ContainsKey(typeof(T))) {
            return null;
        }
        return ComponentMap[typeof(T)][0] as T;
    }
    public IComponent GetComponent(Type type) {
        if (!ComponentMap.TryGetValue(type, out var value)) {
            return null;
        }
        if (value.Count == 0) {
            return null;
        }
        return value[0];
    }

    public List<T> GetComponents<T>() where T : class, IComponent {
        return !ComponentMap.ContainsKey(typeof(T))
            ? []
            : ComponentMap[typeof(T)].Cast<T>().ToList();
    }

    public IEnumerable<IComponent> ActiveComponents() {
        return Components.Where(component => component?.Active != false);
    }

    public IEnumerable<T> ComponentsImplementing<T>() {
        return Components.Where(c => c is T).Cast<T>();
    }

    public IEnumerable<T> RecursiveComponentsImplementing<T>() {
        return ComponentsImplementing<T>()
           .Concat(Children.SelectMany(c => c.RecursiveComponentsImplementing<T>()))
           .Distinct();
    }

    public bool HasComponent(Component component) => Components.Contains(component);
    public bool HasComponent(Type      type)      => Components.Exists(c => c.GetType() == type);

    public bool HasComponent<T>() {
        foreach (var component in Components) {
            if (component is T) {
                return true;
            }
        }

        return false;
    }

}