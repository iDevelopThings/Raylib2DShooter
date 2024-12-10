using System.Diagnostics;
using System.Reflection;

namespace RLShooter.Common.Utils.Reflection;

public static class ReflectionExtensions {
    private static readonly Func<PropertyInfo, bool> IsInstance       = (PropertyInfo property) => !(property.GetMethod ?? property.SetMethod).IsStatic;
    private static readonly Func<PropertyInfo, bool> IsInstancePublic = (PropertyInfo property) => IsInstance(property) && (property.GetMethod ?? property.SetMethod).IsPublic;

    public static bool IsSubclassOfInclGenerics(this Type type, Type other) {
        // if (type.BaseType is {IsGenericType: true} && type.BaseType.GetGenericTypeDefinition() == typeof(GlobalSingletonSystemBase<>)) {
        if (type.BaseType() is {IsGenericType: true} && type.BaseType().GetGenericTypeDefinition() == other) {
            return true;
        }

        return other.IsAssignableFrom(type);
    }

    public static IEnumerable<PropertyInfo> GetProperties(this Type type, bool includeNonPublic) {
        var predicate = includeNonPublic ? IsInstance : IsInstancePublic;

        return type.IsInterface()
            ? (new Type[] {type})
           .Concat(type.GetInterfaces())
           .SelectMany(i => i.GetRuntimeProperties().Where(predicate))
            : type.GetRuntimeProperties().Where(predicate);
    }
    public static PropertyInfo GetPublicProperty(this Type type, string name) {
        return type.GetRuntimeProperty(name);
    }
    public static Type BaseType(this Type type) {
        return type.GetTypeInfo().BaseType;
    }

    public static bool IsValueType(this Type type) {
        return type.GetTypeInfo().IsValueType;
    }

    public static bool IsGenericType(this Type type) {
        return type.GetTypeInfo().IsGenericType;
    }

    public static bool IsGenericTypeDefinition(this Type type) {
        return type.GetTypeInfo().IsGenericTypeDefinition;
    }

    public static bool IsInterface(this Type type) {
        return type.GetTypeInfo().IsInterface;
    }

    public static bool IsEnum(this Type type) {
        return type.GetTypeInfo().IsEnum;
    }

    public static bool IsRequired(this MemberInfo member) {
        var result = member.GetCustomAttributes<System.Runtime.CompilerServices.RequiredMemberAttribute>().Any();
        return result;
    }

    public static Attribute[] GetAllCustomAttributes<TAttribute>(this PropertyInfo member) {
        // IMemberInfo.GetCustomAttributes ignores it's "inherit" parameter for properties,
        // and the suggested replacement (Attribute.GetCustomAttributes) is not available
        // on netstandard1.3
        var result = new List<Attribute>();
        var type   = member.DeclaringType;
        var name   = member.Name;

        while (type != null) {
            var property = type.GetPublicProperty(name);

            if (property != null) {
                result.AddRange(property.GetCustomAttributes(typeof(TAttribute)));
            }

            type = type.BaseType();
        }

        return result.ToArray();
    }

    public static object ReadValue(this PropertyInfo property, object target) {
        return property.GetValue(target, null);
    }

}

public static class ReflectionStore {
    public static bool IsLoading { get; }

    public static readonly Dictionary<Type, List<Type>> ReflectedTypes = new();

    // Groups types by attribute, for ex
    // [AssetTypeDefinition] -> [Scene, Prefab, ScriptableObject]
    public static readonly Dictionary<Type, List<Type>> TypesWithAttribute = new();

    public static readonly List<Type> AllTypes = new();

    public static readonly HashSet<Type> ProjectAssemblyTypes = new();

    public static readonly List<string> ProjectAssemblyNames = new() {
        "RLShooter",
        "RLShooter-Common",
        Assembly.GetEntryAssembly()?.GetName().Name,
    };


    static ReflectionStore() {
        var timer = Stopwatch.StartNew();

        IsLoading = true;

        ProjectAssemblyNames = ProjectAssemblyNames.Distinct().ToList();

        LoadTypes();

        timer.Stop();

        Console.WriteLine($"Loaded {AllTypes.Count} types in {timer.Elapsed}");

        IsLoading = false;
    }
    private static Task LoadTypesAsync() {
        return Task.Run(LoadTypes);
        /*foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            try {
                AddTypes(assembly, assembly.GetTypes());
            }
            catch (ReflectionTypeLoadException e) {
                AddTypes(assembly, e.Types);
            }
        }*/
    }

    private static void LoadTypes() {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies) {
            try {
                AddTypes(assembly, assembly.GetTypes());
            }
            catch (ReflectionTypeLoadException e) {
                AddTypes(assembly, e.Types);
            }
        }
    }


    private static void AddTypes(Assembly assembly, Type[] types) {
        foreach (var type in types) {
            if (type == null)
                continue;

            AllTypes.Add(type);

            if (ProjectAssemblyNames.Contains(type.Assembly.GetName().Name)) {
                ProjectAssemblyTypes.Add(type);
            }

            if (!ReflectedTypes.ContainsKey(type))
                ReflectedTypes[type] = new List<Type>();

            var attributes = type.GetCustomAttributes();
            foreach (var attribute in attributes) {
                if (!TypesWithAttribute.ContainsKey(attribute.GetType()))
                    TypesWithAttribute[attribute.GetType()] = new List<Type>();

                TypesWithAttribute[attribute.GetType()].Add(type);
            }

            foreach (var t in types) {
                if (type.IsAssignableFrom(t))
                    ReflectedTypes[type].Add(t);
                // Also support generic types, for example adding children of SomeType<T> to SomeType<>
                if (type.IsGenericType && t.IsGenericType && type.GetGenericTypeDefinition() == t.GetGenericTypeDefinition())
                    ReflectedTypes[type].Add(t);
            }
        }
    }

    public static List<Type> AllTypesWithAttribute<T>()
        => TypesWithAttribute.GetValueOrDefault(typeof(T), new List<Type>());

    public static List<Type> AllTypesWithAttribute<TA, TB>()
        => TypesWithAttribute.GetValueOrDefault(typeof(TA), new List<Type>())
           .Concat(TypesWithAttribute.GetValueOrDefault(typeof(TB), new List<Type>()))
           .ToList();

    public static List<Type> AllTypesOf<T>() {
        return ReflectedTypes.GetValueOrDefault(typeof(T), new List<Type>());
    }
}

/*public static class AttributeTargetHelperExtension
{

    public static object GetTarget<TAttribute>(this TAttribute attribute)
        where TAttribute : Attribute
    {
        if (ReflectionStore.TypesWithAttribute.TryGetValue(typeof(TAttribute), out var types)) {

        }
    }
}*/