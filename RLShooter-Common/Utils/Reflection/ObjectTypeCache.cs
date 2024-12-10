using System.Reflection;

namespace RLShooter.Common.Utils.Reflection;

public class ObjectTypeCache {
    public Type            Type            { get; set; }
    public List<Attribute> ClassAttributes { get; set; } = new();
    
    
    public List<ObjectTypeCacheMember>               AllFields { get; set; } = new();
    public Dictionary<string, ObjectTypeCacheMember> Fields    { get; set; } = new();

    public ObjectTypeCache() { }
    public ObjectTypeCache(Type type) {
        Type = type;

        ClassAttributes.AddRange(type.GetCustomAttributes());

        var inst = this;
        AllFields.AddRange(
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
               .Select(property => new ObjectTypeCacheMember(this, property))
        );
        AllFields.AddRange(
            type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
               .Select(field => new ObjectTypeCacheMember(this, field))
        );

        Fields = AllFields.ToDictionary(member => member.MemberInfo.Name);
    }

    public IEnumerator<ObjectTypeCacheMember> GetEnumerator() => AllFields.GetEnumerator();

    // Casted T Attribute iterator
    public IEnumerable<T> GetClassAttributes<T>() where T : Attribute
        => ClassAttributes.OfType<T>();

    public static ObjectTypeCache<T> Get<T>() => ObjectTypeCache<T>.Instance;
    
    public static ObjectTypeCache Get(Type type) {
        // Get `Instance` via reflection
        var instance = typeof(ObjectTypeCache<>)
           .MakeGenericType(type)
           .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)?
           .GetValue(null);
        
        return (ObjectTypeCache) instance;
    }
}

public class ObjectTypeCache<TClass> : ObjectTypeCache {

    private static ObjectTypeCache<TClass> _instance;
    public static ObjectTypeCache<TClass> Instance => _instance ??= new ObjectTypeCache<TClass>(typeof(TClass));
    
    public ObjectTypeCache() { }
    public ObjectTypeCache(Type type) : base(type) { }

}