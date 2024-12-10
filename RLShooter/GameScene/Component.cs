using RLShooter.App.Editor.Windows.Inspectors;
using RLShooter.App.Profiling;
using RLShooter.GameScene.Components;

namespace RLShooter.GameScene;

public interface IDrawDebug : IComponent {
    void DrawDebug(GameTime time, Camera2D camera);
}

public interface IRender2D : IComponent {
    void Render2D(GameTime time, Camera2D camera);
}

public interface IRenderUI : IComponent {
    void RenderUI(GameTime time);
}

public interface IComponent {
    public Scene       Scene       { get; }
    public Transform2D Transform2D { get; }
    public GameObject  Owner       { get; set; }
    public bool        Active      { get; set; }

    void OnAddedToGameObject();
    void OnRemovedFromGameObject();
    void Awake();
    void BeginPlay();
    void Update(GameTime time);
    // void Render2D(GameTime  time, Camera2D        camera);
    // void DrawDebug(GameTime time, Camera2D        camera);
    // void RenderUI(GameTime  time);
    void Destroy();

}

public class Component : IComponent, IDisposable {
    [HideInInspector]
    public Scene Scene => Owner?.Scene;
    [HideInInspector]
    public Transform2D Transform2D => Owner?.Transform2D;

    [InspectorShowNameOnly]
    public GameObject Owner { get; set; }

    public bool Active { get; set; } = true;

    public virtual void OnAddedToGameObject()     { }
    public virtual void OnRemovedFromGameObject() { }

    public virtual void Awake()               { }
    public virtual void BeginPlay()           { }
    public virtual void Update(GameTime time) {
    }

    public virtual void Destroy() {
        Owner?.RemoveComponent(this);
    }
    public virtual void Dispose() {
        Destroy();
    }
}