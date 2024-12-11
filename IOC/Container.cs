using System.Reflection;
using System.Runtime.CompilerServices;

namespace IOC;

public enum ContainerRegistrationType {
    Transient,
    Singleton,
}

public class ContainerRegistration {
    public Type Interface      { get; }
    public Type Implementation { get; }

    public ContainerRegistrationType RegistrationType { get; set; }

    public Func<object> Factory { get; set; } = null!;

    public object Instance { get; set; } = null!;

    public ContainerRegistration(Type @interface, Type implementation, ContainerRegistrationType registrationType) {
        Interface        = @interface;
        Implementation   = implementation;
        RegistrationType = registrationType;
    }

    public ContainerRegistration AsSingleton() {
        RegistrationType = ContainerRegistrationType.Singleton;
        return this;
    }
    public ContainerRegistration AsTransient() {
        RegistrationType = ContainerRegistrationType.Transient;
        return this;
    }

    public bool IsDisposable() => Interface.IsAssignableTo(typeof(IDisposable)) || Implementation.IsAssignableTo(typeof(IDisposable));
}

public class Container : IDisposable {
    // Interface Type -> Registration
    // IE, IMyService -> MyServiceA, MyServiceB
    private Dictionary<Type, List<ContainerRegistration>> _registrations = new();

    // Interface Type -> Implementation Type
    // Implementation Type -> Interface Type
    private Dictionary<Type, HashSet<Type>> _typeMappings = new();
    private List<ContainerRegistration>  _disposables  = new();

    private void AddRegistration(ContainerRegistration registration) {
        if (!_registrations.TryGetValue(registration.Interface, out var list)) {
            list                                   = new List<ContainerRegistration>();
            _registrations[registration.Interface] = list;
        }

        list.Add(registration);

        if (registration.IsDisposable()) {
            _disposables.Add(registration);
        }
    }

    private void RegisterMapping(Type serviceType, Type implementationType) {
        if (!_typeMappings.TryGetValue(serviceType, out var list)) {
            list                       = [];
            _typeMappings[serviceType] = list;
        }

        list.Add(implementationType);
    }

    public ContainerRegistration Register<TImplementation>() where TImplementation : class {
        var implType = typeof(TImplementation);

        var reg = new ContainerRegistration(
            implType,
            implType,
            ContainerRegistrationType.Transient
        ) {
            Factory = () => {
                var state = new InstanceResolveState {
                    CurrentType = implType,
                };
                return CreateInstance(implType, ref state);
            },
        };

        AddRegistration(reg);

        // Create links between interfaces and implementations
        foreach (var @interface in implType.GetInterfaces()) {
            RegisterMapping(@interface, implType);
            RegisterMapping(implType, @interface);
        }

        // Create links between base classes and implementations
        var baseType = implType.BaseType;
        while (baseType != null && baseType != typeof(object)) {
            RegisterMapping(baseType, implType);
            RegisterMapping(implType, baseType);
            baseType = baseType.BaseType;
        }


        return reg;
    }

    public ContainerRegistration Register<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService {

        var serviceType = typeof(TService);
        var implType    = typeof(TImplementation);

        var reg = new ContainerRegistration(
            serviceType,
            implType,
            ContainerRegistrationType.Transient
        ) {
            // Factory = Resolve<TImplementation>,
        };

        AddRegistration(reg);

        RegisterMapping(serviceType, implType);
        RegisterMapping(implType, serviceType);

        return reg;
    }

    private T GetOrCreateRegistrationImplementation<T>(ContainerRegistration registration) {
        if (registration.RegistrationType == ContainerRegistrationType.Singleton) {
            if (registration.Instance != null)
                return (T) registration.Instance;

            var instance = registration.Factory != null
                ? registration.Factory()
                : CreateInstance(registration.Implementation);

            registration.Instance = instance;

            // if our reg.Interface != reg.Implementation, then we should check
            // for mappings and set the instance on the mapped registrations
            
            if (_typeMappings.TryGetValue(registration.Interface, out var mappings)) {
                foreach (var mapping in mappings) {
                    if (_registrations.TryGetValue(mapping, out var regs)) {
                        foreach (var reg in regs) {
                            if (reg.Instance != null) continue;
                            if (reg.RegistrationType != ContainerRegistrationType.Singleton) continue;
                            // We need to make sure that the implementation/service type combo is valid
                            if (reg.Implementation != registration.Implementation) continue;
                            
                            reg.Instance = instance;
                        }
                    }
                }
            }
            
            
            return (T) instance;
        }

        if (registration.Factory != null) {
            return (T) registration.Factory();
        }

        // If we don't have a factory, we need to create the instance of the implementation
        var inst = (T) CreateInstance(registration.Implementation);

        return inst;
    }

    private object ResolveRegistration(Type type) {
        if (_registrations.TryGetValue(type, out var registrations)) {
            return GetOrCreateRegistrationImplementation<object>(registrations.Last());
        }

        // If we can't find a registration for the type, try to find a mapping
        if (_typeMappings.TryGetValue(type, out var mappings)) {
            if (_registrations.TryGetValue(mappings.First(), out var regs)) {
                return GetOrCreateRegistrationImplementation<object>(regs.Last());
            }
        }

        throw new InvalidOperationException($"No registration for {type.Name} found.");
    }
    private T ResolveRegistration<T>() => ResolveRegistration(typeof(T)) is T instance ? instance : default;

    private IEnumerable<object> ResolveRegistrations(Type type) {

        if (_registrations.TryGetValue(type, out var registrations)) {
            foreach (var reg in registrations) {
                yield return GetOrCreateRegistrationImplementation<object>(reg);
            }

            yield break;
        }

        // If we can't find a registration for the type, try to find a mapping
        if (_typeMappings.TryGetValue(type, out var mappings)) {
            foreach (var mapping in mappings) {
                if (_registrations.TryGetValue(mapping, out var regs)) {
                    foreach (var reg in regs) {
                        yield return GetOrCreateRegistrationImplementation<object>(reg);
                    }
                }
            }


            yield break;
        }

        throw new InvalidOperationException($"No registration for {type.Name} found.");
    }
    private IEnumerable<T> ResolveRegistrations<T>() {
        var type = typeof(T);
        return ResolveRegistrations(type).Cast<T>();
    }

    public T      Resolve<T>()       => ResolveRegistration<T>();
    public object Resolve(Type type) => ResolveRegistration(type);

    public bool IsResolvable(Type type) => _registrations.ContainsKey(type) && _registrations[type].Count != 0;
    public bool IsResolvable<T>()       => IsResolvable(typeof(T));

    public IEnumerable<TService> ResolveAll<TService>() => ResolveRegistrations<TService>();
    public IEnumerable<object>   ResolveAll(Type type)  => ResolveRegistrations(type);

    private struct CtorFailureInfo {
        public CtorFailureInfo() {
            Constructor = null;
        }

        public ConstructorInfo     Constructor { get; set; }
        public List<ParameterInfo> Parameters  { get; set; } = new();
    }

    internal struct InstanceResolveState {
        public InstanceResolveState() { }

        public Type CurrentType { get; set; } = null!;

        public Queue<Type> TypesResolveQueue { get; set; } = new();
        public List<Type>  TypesResolved     { get; set; } = new();
    }

    internal object CreateInstance(Type type) {
        var state = new InstanceResolveState {
            CurrentType = type,
        };

        return CreateInstance(type, ref state);
    }
    internal object CreateInstance(Type type, ref InstanceResolveState state) {
        var constructors = type.GetConstructors();
        if (constructors.Length == 0) {
            // Create an instance of a class with no constructor
            var inst = Activator.CreateInstance(type)!;

            state.TypesResolved.Add(type);

            return inst;
        }

        // Sort by most parameters
        constructors = constructors
           .OrderByDescending(c => c.GetParameters().Length)
           .ToArray();

#if DEBUG
        var ctorFailures = new List<CtorFailureInfo>();
#endif

        // Try to find the constructor which matches parameters available in the container
        foreach (var constructor in constructors) {
            var parameters = constructor.GetParameters();
            if (parameters.Length == 0) {
                var inst = Activator.CreateInstance(type)!;
                state.TypesResolved.Add(type);
                return inst;
            }

#if DEBUG
            var ctorFailure = new CtorFailureInfo {
                Constructor = constructor,
            };
#endif
            var canResolve        = true;
            var paramResolveQueue = new Queue<ParameterInfo>();

            // Ensure that we can resolve all parameters
            foreach (var param in parameters) {
                if (!IsResolvable(param.ParameterType)) {
#if DEBUG
                    ctorFailure.Parameters.Add(param);
#endif
                    canResolve = false;
                    continue;
                }

                paramResolveQueue.Enqueue(param);
            }

            if (!canResolve) {
#if DEBUG
                ctorFailures.Add(ctorFailure);
#endif
                continue;
            }

            var paramInstances = new List<object>();
            while (paramResolveQueue.Count > 0) {
                var param         = paramResolveQueue.Dequeue();
                var paramInstance = ResolveParameter(param.ParameterType);
                paramInstances.Add(paramInstance);
            }

            var instance = constructor.Invoke(paramInstances.ToArray());
            state.TypesResolved.Add(type);
            return instance;
        }


        var errorMessage = $"No suitable constructor found for {type.Name}";

#if DEBUG
        if (ctorFailures.Count > 0) {
            errorMessage += "\n\n";
            errorMessage += "Failed constructors:\n";
            foreach (var ctorFailure in ctorFailures) {
                errorMessage += $"Constructor: {ctorFailure.Constructor}\n";
                errorMessage += "Parameters failed:\n";
                errorMessage = ctorFailure.Parameters.Aggregate(
                    errorMessage,
                    (current, parameter) => current + $"  {parameter.ParameterType.Name} {parameter.Name} (not resolvable)\n"
                );
            }
        }
#endif

        throw new InvalidOperationException(errorMessage);
    }

    private object ResolveParameter(Type parameterType) {
        if (_registrations.TryGetValue(parameterType, out var registrations)) {
            return GetOrCreateRegistrationImplementation<object>(registrations.First());
        }

        throw new InvalidOperationException($"No registration for {parameterType.Name} found.");
    }

    public void Dispose() {
        foreach (var reg in _disposables) {
            if (reg.Instance is IDisposable disposable) {
                disposable.Dispose();
            }
        }
        _disposables.Clear();
        _registrations.Clear();
    }
}