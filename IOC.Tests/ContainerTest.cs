using IOC;
using JetBrains.Annotations;

namespace IOC.Tests;

[TestSubject(typeof(Container))]
public class ContainerTest {

    [Fact]
    public void Register_Transient_ResolvesDifferentInstances() {
        using var container = new Container();

        container.Register<MyService>();

        var instance1 = container.Resolve<MyService>();
        var instance2 = container.Resolve<MyService>();

        Assert.NotSame(instance1, instance2);
    }

    [Fact]
    public void Register_Singleton_ResolvesSameInstance() {
        using var container = new Container();

        container.Register<MyService>().AsSingleton();

        var instance1 = container.Resolve<MyService>();
        var instance2 = container.Resolve<MyService>();

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void Register_Singleton_ResolvesViaInterface() {
        using var container = new Container();

        container.Register<IMyService, MyService>().AsSingleton();
        container.Register<MyService>().AsSingleton();

        var instance1 = container.Resolve<MyService>();
        var instance2 = container.Resolve<MyService>();
        var instance3 = container.Resolve<IMyService>();
        var instance4 = container.Resolve<IMyService>();

        Assert.Same(instance1, instance2);
        Assert.Same(instance3, instance4);
        Assert.Same(instance1, instance3);
        Assert.Same(instance2, instance4);
    }

    [Fact]
    public void Register_Interface_ResolvesImplementation() {
        using var container = new Container();

        container.Register<IMyService, MyService>();

        var instance = container.Resolve<IMyService>();

        Assert.IsType<MyService>(instance);
    }

    [Fact]
    public void Resolve_UnregisteredType_ThrowsException() {
        using var container = new Container();

        Assert.Throws<InvalidOperationException>(() => container.Resolve<MyService>());
    }

    [Fact]
    public void ResolveAll_ResolvesAllRegisteredInstances() {
        using var container = new Container();

        container.Register<IMyService, MyServiceA>();
        container.Register<IMyService, MyServiceB>();
        
        container.Register<MyService_Ext_A>();
        container.Register<MyService_Ext_B>();

        var instances = container.ResolveAll<IMyService>();

        Assert.Collection(
            instances,
            item => Assert.IsType<MyServiceA>(item),
            item => Assert.IsType<MyServiceB>(item)
        );

        // We should also be able to resolve sub types
        // MyService_Ext_A & MyService_Ext_B are subtypes of MyService
        instances = container.ResolveAll<MyService>().ToList();
        
        Assert.Collection(
            instances,
            item => Assert.IsType<MyService_Ext_A>(item),
            item => Assert.IsType<MyService_Ext_B>(item)
        );
        
    }

    [Fact]
    public void CreateInstance_NoConstructor_CreatesInstance() {
        using var container = new Container();

        var instance = container.CreateInstance(typeof(NoConstructorService));

        Assert.IsType<NoConstructorService>(instance);
    }

    [Fact]
    public void CreateInstance_ConstructorWithResolvableParameters_CreatesInstance() {
        using var container = new Container();

        container.Register<DependencyService>();
        
        var instance = container.CreateInstance(typeof(ServiceWithDependency));

        Assert.IsType<ServiceWithDependency>(instance);
    }

    [Fact]
    public void CreateInstance_ConstructorWithUnresolvableParameters_ThrowsException() {
        using var container = new Container();
        
        Assert.Throws<InvalidOperationException>(() => container.CreateInstance(typeof(ServiceWithDependency)));
    }
}

public interface IMyService { }
public class MyService : IMyService { }
public class MyServiceA : IMyService { }
public class MyServiceB : IMyService { }
public class MyService_Ext_A : MyService { }
public class MyService_Ext_B : MyService { }
public class NoConstructorService { }
public class DependencyService { }

public class ServiceWithDependency {
    public ServiceWithDependency(DependencyService dependency) { }
}

