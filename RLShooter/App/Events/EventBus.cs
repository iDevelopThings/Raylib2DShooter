namespace RLShooter.App.Events;

public class EventBus
{
    private readonly Dictionary<Type, List<Delegate>> EventSubscribers = new();

    // Subscribe to an event of type T (where T is a struct)
    public void Subscribe<T>(Action<T> subscriber) where T : struct {
        var eventType = typeof(T);

        if (!EventSubscribers.TryGetValue(eventType, out var value)) {
            value = ([]);
            EventSubscribers[eventType] = value;
        }

        value.Add(subscriber);
    }

    // Unsubscribe from an event of type T (where T is a struct)
    public void Unsubscribe<T>(Action<T> subscriber) where T : struct {
        var eventType = typeof(T);

        if (!EventSubscribers.TryGetValue(eventType, out var value))
            return;

        value.Remove(subscriber);

        if (value.Count == 0) {
            EventSubscribers.Remove(eventType);
        }
    }

    // Publish an event of type T (where T is a struct)
    public void Publish<T>(T eventToPublish) where T : struct {
        var eventType = typeof(T);

        if (!EventSubscribers.TryGetValue(eventType, out var value))
            return;

        foreach (var subscriber in value) {
            ((Action<T>) subscriber)?.Invoke(eventToPublish);
        }
    }
}

public static class GlobalEventBus
{
    public static EventBus Instance { get; } = new EventBus();
    
    public static void Subscribe<T>(Action<T> subscriber) where T : struct {
        Instance.Subscribe(subscriber);
    }
    
    public static void Unsubscribe<T>(Action<T> subscriber) where T : struct {
        Instance.Unsubscribe(subscriber);
    }
    
    public static void Publish<T>(T eventToPublish) where T : struct {
        Instance.Publish(eventToPublish);
    }
}