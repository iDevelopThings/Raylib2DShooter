using Arch.Core;
using Arch.System;
using DryIoc;
using RLShooter.Common.Utils.Reflection;

namespace RLShooter.App;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class ContainerSingletonAttribute : Attribute { }

public abstract class SingletonBase {
    public static void RegisterAllWithContainer(Container container) {
        var all = ReflectionStore.AllTypesWithAttribute<ContainerSingletonAttribute>()
           .Where(type => !type.IsAbstract)
           .ToList();

        foreach (var type in all) {
            container.Register(type, Reuse.Singleton);
        }

        /*foreach (var type in all) {
            if (!type.IsAssignableTo(typeof(ISceneSystem)))
                continue;

            var inst = container.Resolve(type) as ISceneSystem;
            if(inst?.Flags.HasFlag(SystemFlags.Registered) == true)
                inst.OnRegistered();

            container.RegisterInstance(typeof(ISceneSystem), inst);
        }*/

    }
}

public class Singleton<T> : SingletonBase where T : IDisposable, new() {
    private static T _instance;
    public static T Instance {
        get {
            if (_instance == null) {
                _instance = Application.Container.Resolve<T>();
            }
            return _instance;
        }
    }
}

public class BaseSystemSingleton<T> : BaseSystem<World, float> {
    private static T _instance;
    public static T Instance {
        get {
            if (_instance == null) {
                _instance = Application.Container.Resolve<T>();
            }
            return _instance;
        }
    }

    public BaseSystemSingleton(World world) : base(world) { }
}