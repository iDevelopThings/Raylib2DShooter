using System.Reflection;

namespace RLShooter.Common.Utils.Reflection;

public struct ObjectTypeCacheMember {
    public ObjectTypeCache OuterType  { get; set; }
    public MemberInfo      MemberInfo { get; set; }

    public Dictionary<Type, List<Attribute>> Attributes { get; set; } = new();

    public ObjectTypeCacheMember(ObjectTypeCache outerType, MemberInfo memberInfo) {
        OuterType  = outerType;
        MemberInfo = memberInfo;

        Attributes = memberInfo.GetCustomAttributes()
           .GroupBy(attr => attr.GetType())
           .ToDictionary(group => group.Key, group => group.ToList());
    }

    public bool IsReadOnly => MemberInfo switch {
        PropertyInfo propertyInfo => !propertyInfo.CanWrite,
        FieldInfo fieldInfo       => false,
        _                         => true,
    };
    public bool IsWriteOnly => MemberInfo switch {
        PropertyInfo propertyInfo => !propertyInfo.CanRead,
        FieldInfo fieldInfo       => false,
        _                         => true,
    };

    public bool CanReadWrite => MemberInfo switch {
        PropertyInfo propertyInfo => propertyInfo is {CanRead: true, CanWrite: true},
        FieldInfo fieldInfo       => true,
        _                         => false
    };

    public T GetValue<T>(object container) => (T) GetValue(container);

    public object GetValue(object container) {
        return MemberInfo switch {
            PropertyInfo propertyInfo => propertyInfo.GetValue(container),
            FieldInfo fieldInfo       => fieldInfo.GetValue(container),
            _                         => default
        };
    }

    public void SetValue(object container, object value) {
        switch (MemberInfo) {
            case PropertyInfo propertyInfo:
                propertyInfo.SetValue(container, value);
                break;
            case FieldInfo fieldInfo:
                fieldInfo.SetValue(container, value);
                break;
        }
    }

    public Type Type => MemberInfo switch {
        PropertyInfo propertyInfo => propertyInfo.PropertyType,
        FieldInfo fieldInfo       => fieldInfo.FieldType,
        _                         => default
    };

    public string Name => MemberInfo.Name;

    public bool           HasAttribute<T>() where T : Attribute  => Attributes.ContainsKey(typeof(T));
    public T              GetAttribute<T>() where T : Attribute  => Attributes.TryGetValue(typeof(T), out var attr) ? attr.Cast<T>().FirstOrDefault() : null;
    public IEnumerable<T> GetAttributes<T>() where T : Attribute => Attributes.TryGetValue(typeof(T), out var attr) ? attr.Cast<T>() : [];
}