using DryIoc;
using RLShooter.App.Editor;
using RLShooter.App.Editor.ImGuiIntegration;
using RLShooter.App.InputManagement;
using RLShooter.Config;
using RLShooter.Gameplay.Systems;
using RLShooter.GameScene;

namespace RLShooter.App;

public static class ContainerExtensions {

    public static T ResolveFromContainer<T>(this T @this) {
        return Application.Container.Resolve<T>(@this.GetType());
    }

}

public static class AppContainer {
    public static void RegisterSingleton(Type type)
        => Application.Container.Register(type, Reuse.Singleton);
    public static T RegisterResolveSingleton<T>() {
        Application.Container.Register<T>(Reuse.Singleton);
        return Application.Container.Resolve<T>();
    }
    public static void RegisterInstance<T>(T instance)
        => Application.Container.RegisterInstance(instance, IfAlreadyRegistered.AppendNotKeyed);

    public static IEnumerable<T> ResolveMany<T>()
        => Application.Container.ResolveMany<T>();

    public static TRequiredService Resolve<TService, TRequiredService>() where TRequiredService : class
        => Application.Container.Resolve<TService, TRequiredService>() as TRequiredService;

    public static T Resolve<T>()
        => Application.Container.Resolve<T>();
    public static object Resolve(Type type)
        => Application.Container.Resolve(type);
    public static T Resolve<T>(Type type)
        => Application.Container.Resolve(type) is T t ? t : default;

    public static T Resolve<T>(this T @this)
        => Application.Container.Resolve<T>(@this.GetType());

}

public class Application {
    public static Window           Window    { get; set; }
    public static Container        Container { get; set; }

    private static bool _shouldQuit = false;
    public static bool ShouldQuit {
        get => Raylib.WindowShouldClose() && !_shouldQuit;
        set => _shouldQuit = value;
    }
    public static bool InPlayMode { get; set; }

    public static Scene CurrentScene {
        get => Scene.Current;
        set => Scene.Current = value;
    }

    public static void Init() {
        BootStrap();
        Window = Container.Resolve<Window>();
    }
    private static void BootStrap() {
        Container = new Container();
        
        Container.Register<Window>(Reuse.Singleton);

        Container.Register<ISceneECSSystem, MovementSystem>(Reuse.Singleton);
        Container.Register<ISceneECSSystem, PlayerControlSystem>(Reuse.Singleton);
        Container.Register<ISceneECSSystem, PhysicsSystem>(Reuse.Singleton);
        Container.Register<ISceneECSSystem, DestroyAfterTimeSystem>(Reuse.Singleton);
        Container.Register<ISceneRenderSystem, RenderSystem>(Reuse.Singleton);
        Container.Register<ISceneRenderSystem, DebugSystem>(Reuse.Singleton);
        Container.Register<ISceneRenderSystem, ImGuiManagerSystem>(Reuse.Singleton);

        SingletonBase.RegisterAllWithContainer(Container);

    }

    public static Scene CreateScene(string name, bool setActive) {
        var scene = new Scene(name);

        Container.RegisterInstance(scene, null, null, scene.Name);

        if (setActive) {
            CurrentScene = scene;
        }

        scene.OnInitialize();

        return scene;
    }
    public static void OnTick(GameTime time) {
        Window.Tick();
        Input.Update();
    }

    public static void Play(Scene scene) { }
    public static void StopPlaying()     { }
}