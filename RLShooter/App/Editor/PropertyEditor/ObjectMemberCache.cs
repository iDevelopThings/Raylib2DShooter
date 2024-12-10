using System.Reflection;
using RLShooter.App.Editor.Windows.Inspectors;

namespace RLShooter.App.Editor.PropertyEditor;

public class ObjectMemberCache {

    public ObjectTypeCache Parent     { get; set; }
    public MemberInfo      MemberInfo { get; set; }

    public Dictionary<Type, List<Attribute>> Attributes { get; set; }

    public int SortOrder { get; set; }

    public ObjectMemberCache(MemberInfo memberInfo, ObjectTypeCache parent = null) {
        MemberInfo = memberInfo;
        Parent     = parent;

        Attributes = memberInfo.GetCustomAttributes()
           .GroupBy(attr => attr.GetType())
           .ToDictionary(group => group.Key, group => group.ToList());
    }

    public bool CanDrawInInspector => MemberInfo.GetCustomAttribute<HideInInspectorAttribute>() == null;
    public bool ShowNameOnly       => MemberInfo.GetCustomAttribute<InspectorShowNameOnly>() != null;
    public bool AdvancedSection    => MemberInfo.GetCustomAttribute<InspectorAdvanced>() != null;

    public string Category => MemberInfo.GetCustomAttribute<InspectorCategory>()?.Category;

    public bool Disabled => HasAttribute<DisabledInInspectorAttribute>() || !CanWrite();

    public T GetValue<T>(object instance) => (T) GetValue(instance);
    public object GetValue(object instance) {
        return MemberInfo switch {
            PropertyInfo propertyInfo => propertyInfo.GetValue(instance),
            FieldInfo fieldInfo       => fieldInfo.GetValue(instance),
            _                         => default
        };
    }

    public bool CanWrite() => MemberInfo switch {
        PropertyInfo propertyInfo => propertyInfo.CanWrite,
        FieldInfo                 => true,
        _                         => false,
    };

    public bool CanRead() => MemberInfo switch {
        PropertyInfo propertyInfo => propertyInfo.CanRead,
        FieldInfo                 => true,
        _                         => false,
    };

    public void SetValue(object value, object instance) {
        switch (MemberInfo) {
            case PropertyInfo propertyInfo:
                propertyInfo.SetValue(instance, value);
                break;
            case FieldInfo fieldInfo:
                fieldInfo.SetValue(instance, value);
                break;
        }
    }

    public Type Type => MemberInfo switch {
        PropertyInfo propertyInfo => propertyInfo.PropertyType,
        FieldInfo fieldInfo       => fieldInfo.FieldType,
        _                         => default,
    };

    public string Name => MemberInfo.Name;

    // public bool HasAttribute<T>() where T : Attribute => MemberInfo.GetCustomAttribute<T>() != null;
    public bool HasAttribute<T>() where T : Attribute => Attributes.ContainsKey(typeof(T));
    public T    GetAttribute<T>() where T : Attribute => Attributes.TryGetValue(typeof(T), out var attr) ? (T) attr[0] : null;
    public bool GetAttribute<T>(out T attribute) where T : Attribute {
        if (Attributes.TryGetValue(typeof(T), out var attr)) {
            attribute = (T) attr[0];
            return true;
        }

        attribute = null;
        return false;
    }
    
    public Vector2 GetMinMax() {
        return GetAttribute<InspectorMinMax>(out var minMax)
            ? new Vector2(minMax.Min, minMax.Max)
            : Vector2.Zero;
    }
}