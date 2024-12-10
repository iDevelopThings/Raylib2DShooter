namespace RLShooter.App.Editor.PropertyEditor;

public class PropertyEditorCache {

    public static Dictionary<Type, ObjectTypeCache> ObjectTypeCaches = new();


    public static ObjectTypeCache Get(Type type) {
        if (ObjectTypeCaches.TryGetValue(type, out var cache)) {
            return cache;
        }

        cache                  = new ObjectTypeCache(type);
        ObjectTypeCaches[type] = cache;

        return cache;
    }
}