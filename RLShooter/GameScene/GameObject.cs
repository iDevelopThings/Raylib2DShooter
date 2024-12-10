using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using RLShooter.App.Editor;
using RLShooter.App.Editor.Windows.Inspectors;
using RLShooter.Config;
using RLShooter.App.Events;
using RLShooter.App.Profiling;
using RLShooter.GameScene.Components;
using RLShooter.Common.Utils.Reflection;

namespace RLShooter.GameScene;

public partial class GameObject : IEditorSelectable {
    [InspectorCategory("GameObject")]
    public Guid AssetId { get; set; } = Guid.NewGuid();

    [InspectorCategory("GameObject")]
    public string Name { get; set; }
    [InspectorCategory("GameObject")]
    public string DebugName => $"{Name} ({AssetId})";

    [InspectorCategory("GameObject"), InspectorShowNameOnly]
    public Scene Scene { get; set; }
    [InspectorCategory("GameObject"), InspectorShowNameOnly]
    public GameObject Parent { get; set; }

    [RequiredComponent]
    public Transform2D Transform2D { get; set; }

    public List<GameObject> Children { get; set; } = new();

    [HideInInspector]
    public List<IComponent> Components = new();
    [HideInInspector]
    public Dictionary<Type, List<IComponent>> ComponentMap = new();

    public enum GameObjectFlags : byte {
        None                 = 0,
        Active               = 1 << 0,
        HasAwoken            = 1 << 1,
        HasBegunPlay         = 1 << 2,
        MarkedForDestruction = 1 << 3,
    }

    public GameObjectFlags ObjectFlags = GameObjectFlags.Active;

    [InspectorCategory("GameObject/States")]
    public bool Active {
        get => (ObjectFlags & GameObjectFlags.Active) != 0;
        set => ObjectFlags = value ? ObjectFlags | GameObjectFlags.Active : ObjectFlags & ~GameObjectFlags.Active;
    }
    [InspectorCategory("GameObject/States")]
    public bool HasAwoken {
        get => (ObjectFlags & GameObjectFlags.HasAwoken) != 0;
        set => ObjectFlags = value ? ObjectFlags | GameObjectFlags.HasAwoken : ObjectFlags & ~GameObjectFlags.HasAwoken;
    }
    [InspectorCategory("GameObject/States")]
    public bool HasBegunPlay {
        get => (ObjectFlags & GameObjectFlags.HasBegunPlay) != 0;
        set => ObjectFlags = value ? ObjectFlags | GameObjectFlags.HasBegunPlay : ObjectFlags & ~GameObjectFlags.HasBegunPlay;
    }
    [InspectorCategory("GameObject/States")]
    public bool MarkedForDestruction {
        get => (ObjectFlags & GameObjectFlags.MarkedForDestruction) != 0;
        set => ObjectFlags = value ? ObjectFlags | GameObjectFlags.MarkedForDestruction : ObjectFlags & ~GameObjectFlags.MarkedForDestruction;
    }

    private enum InternalEditorFlags : byte {
        None = 0,

        /// <summary>
        /// Is selected in the scene hierarchy widget.
        /// </summary>
        Selected = 1 << 0,

        /// <summary>
        /// Is open in the scene hierarchy widget.
        /// </summary>
        Open = 1 << 1,

        /// <summary>
        /// Is displayed in the scene hierarchy widget.
        /// </summary>
        Displayed = 1 << 2,
    }

    private InternalEditorFlags _editorFlags = InternalEditorFlags.None;

    [InspectorCategory("GameObject/States/EditorFlags")]
    public bool IsSelectedInEditor {
        get => (_editorFlags & InternalEditorFlags.Selected) != 0;
        set => _editorFlags = value ? _editorFlags | InternalEditorFlags.Selected : _editorFlags & ~InternalEditorFlags.Selected;
    }
    [InspectorCategory("GameObject/States/EditorFlags")]
    public bool OpenInEditor {
        get => (_editorFlags & InternalEditorFlags.Open) != 0;
        set => _editorFlags = value ? _editorFlags | InternalEditorFlags.Open : _editorFlags & ~InternalEditorFlags.Open;
    }
    [InspectorCategory("GameObject/States/EditorFlags")]
    public bool DisplayedInEditor {
        get => (_editorFlags & InternalEditorFlags.Displayed) != 0;
        set => _editorFlags = value ? _editorFlags | InternalEditorFlags.Displayed : _editorFlags & ~InternalEditorFlags.Displayed;
    }


    private readonly EventHandlers<GameObject, GameObject> onDestroyListeners = new();
    public event EventHandler<GameObject, GameObject> OnDestroy {
        add => onDestroyListeners.AddHandler(value);
        remove => onDestroyListeners.RemoveHandler(value);
    }

    private readonly EventHandlers<GameObject, Transform2D> transformChangedListeners = new();
    public event EventHandler<GameObject, Transform2D> TransformChanged {
        add => transformChangedListeners.AddHandler(value);
        remove => transformChangedListeners.RemoveHandler(value);
    }
    private readonly EventHandlers<GameObject, Transform2D> transformUpdatedListeners = new();
    public event EventHandler<GameObject, Transform2D> TransformUpdated {
        add => transformUpdatedListeners.AddHandler(value);
        remove => transformUpdatedListeners.RemoveHandler(value);
    }

    public GameObject(string name, GameObject parent = null, Scene scene = null) {
        Name   = name;
        Scene  = scene;
        Parent = parent;

        OnConstruct();

        Transform2D.OnTransformChanged += OnTransformChanged;
        Transform2D.OnTransformUpdated += OnTransformUpdated;

        // Scene?.RegisterGameObject(this);

        // Generate an asset id guid from the provided name
        if (!string.IsNullOrEmpty(name)) {
            AssetId = new Guid(MD5.HashData(Encoding.UTF8.GetBytes(name)));

            if (AppConfig.IsEditorUiEnabled) {
                if (AssetId == AppConfig.LastSelectedObjectId) {
                    SelectionCollection.Global.AddOverwriteSelection(this);
                }
            }
        }

    }

    protected virtual void OnConstruct() {
        // Find all `RequiredComponentAttribute` and create components
        var props = ObjectTypeCache.Get(GetType());
        foreach (var field in props.AllFields) {
            var attr = field.GetAttribute<RequiredComponentAttribute>();
            if (attr == null)
                continue;

            var component = AddComponent(field.Type);
            field.SetValue(this, component);
        }

        // Now... we'll check all components for `RequiredComponentAttribute` and create components

        foreach (var component in Components) {
            if (component == null)
                continue;

            var compProps = ObjectTypeCache.Get(component.GetType());
            foreach (var field in compProps.AllFields) {
                var attr = field.GetAttribute<RequiredComponentAttribute>();
                if (attr == null)
                    continue;

                if (field.GetValue(component) != null)
                    continue;

                // If this component we want exists on our object already, use this instance
                var existing = GetComponent(field.Type);
                if (existing != null) {
                    field.SetValue(component, existing);
                    continue;
                }

                // Otherwise, create a new component
                var newComponent = AddComponent(field.Type);
                field.SetValue(component, newComponent);
            }

        }


    }

    public void AddChild(GameObject child) {
        child.Parent = this;
        Children.Add(child);
    }
    public void RemoveChild(GameObject child) {
        child.Parent = null;

        Children.Remove(child);
    }

    private void OnTransformChanged(Transform2D transform) {
        transformChangedListeners.Invoke(this, transform);
    }
    private void OnTransformUpdated(Transform2D transform) {
        transformUpdatedListeners.Invoke(this, transform);
    }

    protected virtual void Awake() {
        foreach (var component in ActiveComponents()) {
            component?.Awake();
        }
    }
    protected virtual void BeginPlay() {
        foreach (var component in ActiveComponents()) {
            component?.BeginPlay();
        }
    }
    
    [Profile("GameObject Tick")]
    public void Tick(GameTime time) {
        if (!Active)
            return;

        if (!HasAwoken) {
            HasAwoken = true;
            Awake();
            return;
        }

        if (!HasBegunPlay) {
            HasBegunPlay = true;
            BeginPlay();
            return;
        }

        TickComponents(time);
    }

    [Profile("GameObject Component Tick")]
    public void TickComponents(GameTime time) {
        // using var _ = new ProfilingScope("Components Tick");

        foreach (var component in Components) {
            if (!component.Active)
                continue;
            component?.Update(time);
        }
    }

    public void Destroy(bool immediate = false) {
        Active               = false;
        MarkedForDestruction = true;
        if (immediate) {
            FinalizeDestruction();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FinalizeDestruction() {
        if (!MarkedForDestruction)
            return;

        var originalComponents = Components.ToArray();
        foreach (var component in originalComponents) {
            component?.Destroy();
        }

        Parent?.RemoveChild(this);

        var originalChildren = Children.ToArray();
        foreach (var child in originalChildren) {
            child.Destroy();
        }

        // Scene?.UnregisterGameObject(this);

        onDestroyListeners?.Invoke(this, this);
    }


    public override string ToString() {
        return DebugName;
    }
}

/*
    public Matrix4x4 GetGlobalMatrix() {
        return Transform.GlobalMatrix;

        /*if (Parent == null)
            return Transform.GetMatrix();

        return Transform.GetMatrix() * Parent.GetGlobalMatrix();#1#
    }

    public Transform GetGlobalTransform(bool reInstantiate = false) {
        // if (GlobalTransform != null)
        //     return GlobalTransform;
        //
        // var globalMatrix = GetGlobalMatrix();
        //
        // if (reInstantiate) {
        //     GlobalTransform = new Transform(globalMatrix);
        // } else {
        //     GlobalTransform ??= new Transform();
        //     GlobalTransform.SetGlobalMatrix(globalMatrix);
        // }
        //
        // return GlobalTransform;
    }

    public Vector3 GetGlobalPosition() {
        return GlobalTransform.Position;
        // var globalMatrix = GetGlobalMatrix();
        // return Vector3.Transform(Vector3.Zero, globalMatrix);
    }

    public Quaternion GetGlobalRotation() {
        return GlobalTransform.Rotation;
        // var globalMatrix = GetGlobalMatrix();
        // return Quaternion.CreateFromRotationMatrix(globalMatrix);
    }

    public Vector3 GetGlobalScale() {
        return GlobalTransform.Scale;
        // var globalMatrix = GetGlobalMatrix();
        // return new Vector3(globalMatrix.M11, globalMatrix.M22, globalMatrix.M33);
    }
    */