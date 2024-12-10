using System.Runtime.CompilerServices;
using RLShooter.App.Editor.Windows.Inspectors;
using RLShooter.App.Profiling;
using RLShooter.Common.Mathematics;
using RLShooter.GameScene.Components;

namespace RLShooter.GameScene;

public delegate void OnTransform2DChanged(Transform2D transform);
public delegate void OnTransform2DUpdated(Transform2D transform);

public class Transform2D : Component/*, IDrawDebug*/ {
    private Vector2 _position = Vector2.Zero;

    private bool      _rotationDirty        = true;
    private float     _rotation             = 0;
    private Matrix3x2 _cachedRotationMatrix = Matrix3x2.Identity;

    private bool      _scaleDirty        = true;
    private Vector2   _scale             = Vector2.One;
    private Matrix3x2 _cachedScaleMatrix = Matrix3x2.Identity;

    private bool    _pivotDirty  = true;
    private Vector2 _pivot       = Vector2.Zero;
    private Vector2 _cachedPivot = Vector2.Zero;

    private Matrix3x2 cachedLocalTransform  = Matrix3x2.Identity;
    private Matrix3x2 cachedGlobalTransform = Matrix3x2.Identity;
    private Vector2   cachedGlobalPosition  = Vector2.Zero;
    private Vector2   cachedGlobalScale     = Vector2.One;
    private float     cachedGlobalRotation  = 0;

    public bool IsDirty     { get; set; } = true;
    public bool LightWeight { get; set; } = false;

    public Vector2 Position {
        get => _position;
        set {
            if (_position == value)
                return;

            _position = value;
            
            if (LightWeight)
                cachedGlobalPosition = _position;

            MarkDirty();
        }
    }

    [InspectorMinMax(0, 360)]
    public float Rotation {
        get => _rotation;
        set {
            if (!(Math.Abs(_rotation - value) > 0.001f))
                return;

            _rotation      = value;
            _rotationDirty = LightWeight == false;

            if (LightWeight)
                cachedGlobalRotation = _rotation;

            MarkDirty();
        }
    }

    public Vector2 Scale {
        get => _scale;
        set {
            if (_scale == value)
                return;

            _scale      = value;
            _scaleDirty = LightWeight == false;
            if (LightWeight)
                cachedGlobalScale = _scale;
            MarkDirty();
        }
    }

    // The pivot point offset relative to the object's local space (e.g., for center rotation)
    [InspectorMinMax(0, 1)]
    public Vector2 Pivot {
        get => _pivot;
        set {
            if (_pivot == value)
                return;
            
            _pivot      = value;
            _pivotDirty = true;

            MarkDirty();
        }
    }



    public Transform2D Parent => Owner.Parent?.Transform2D;

    // Occurs when we change something which affects the global transform
    public event OnTransform2DChanged OnTransformChanged;
    // Occurs when our transform has actually been recalculated and updated
    public event OnTransform2DUpdated OnTransformUpdated;

    public Transform2D(GameObject owner) {
        Owner = owner;
        MarkDirty();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Profile("Transform Recalculation")]
    public void Recalculate(bool force = false) {
        if (!IsDirty && !force)
            return;

        // using var _ = new ProfilingScope("Global Transform ReCalculation");


        // IF we're using LightWeight mode, we assume there is no "parent", and just use the local transform as the global transform


        if (LightWeight) {
            // No parent-child relationship, just use direct calculations
            cachedGlobalPosition = Position; // No pivot adjustment
            cachedGlobalScale    = Scale;    // Direct scaling
            cachedGlobalRotation = Rotation; // Direct rotation

            // Optional: Precompute a "lightweight global transform" if needed for rendering
            /*var radians = MathF.PI / 180 * Rotation;
            var cos     = MathF.Cos(radians);
            var sin     = MathF.Sin(radians);

            cachedGlobalTransform = new Matrix3x2(
                cos * Scale.X, sin * Scale.X,
                -sin * Scale.Y, cos * Scale.Y,
                Position.X, Position.Y
            );*/

            OnTransformUpdated?.Invoke(this);

            IsDirty = false;
            return;
        }


        // Pivot will be 0-1, so we need to use `GetBounds` to get the actual pivot point
        // Recalculate pivot only if necessary
        if (_pivotDirty) {
            _cachedPivot = Bounds.Size * (Point2) Pivot;
            _pivotDirty  = false;
        }
        var pivot = _cachedPivot;

        // Precompute matrices if needed
        if (_rotationDirty) {
            _cachedRotationMatrix = Matrix3x2.CreateRotation(MathF.PI / 180 * Rotation);
            _rotationDirty        = false;
        }

        if (_scaleDirty) {
            _cachedScaleMatrix = Matrix3x2.CreateScale(Scale);
            _scaleDirty        = false;
        }
        var parentTransform = Parent?.cachedGlobalTransform ?? Matrix3x2.Identity;
        // Combine transformations
        cachedLocalTransform = Matrix3x2.CreateTranslation(-pivot) *
                               _cachedScaleMatrix *
                               _cachedRotationMatrix *
                               Matrix3x2.CreateTranslation(pivot + Position);

        // Global transformations
        cachedGlobalTransform = cachedLocalTransform * parentTransform;
        cachedGlobalPosition  = Vector2.Transform(Vector2.Zero, cachedGlobalTransform);

        var parentGlobalScale    = Parent?.GlobalScale ?? Vector2.One;
        var parentGlobalRotation = Parent?.GlobalRotation ?? 0;

        cachedGlobalScale    = Scale * parentGlobalScale;
        cachedGlobalRotation = Rotation + parentGlobalRotation;

        OnTransformUpdated?.Invoke(this);

        IsDirty = false;

        /*var pivotTranslation = Matrix3x2.CreateTranslation(-pivot);
        var rotationMatrix   = Matrix3x2.CreateRotation(MathF.PI / 180 * Rotation);
        var scaleMatrix      = Matrix3x2.CreateScale(Scale);
        var finalTranslation = Matrix3x2.CreateTranslation(pivot + Position);

        // Combine transformations in the correct order
        cachedLocalTransform = pivotTranslation * scaleMatrix * rotationMatrix * finalTranslation;

        // If there's no parent, this is the global transform
        if (Parent == null) {
            cachedGlobalTransform = cachedLocalTransform;
        } else {
            cachedGlobalTransform = cachedLocalTransform * Parent.cachedGlobalTransform;
        }

        cachedGlobalPosition = Vector2.Transform(Vector2.Zero, cachedGlobalTransform);
        cachedGlobalScale    = Scale * (Parent?.GlobalScale ?? Vector2.One);
        cachedGlobalRotation = Rotation + (Parent?.GlobalRotation ?? 0);

        OnTransformUpdated?.Invoke(this);

        IsDirty = false;*/
    }

    // Mark this transform and all children as dirty
    public void MarkDirty() {
        if (IsDirty)
            return;
        
        if (LightWeight)
            return;

        IsDirty = true;
        OnTransformChanged?.Invoke(this);

        if (Owner == null)
            return;

        foreach (var child in Owner.Children) {
            child?.Transform2D.MarkDirty();
        }
    }

    // Calculate the global (world) transformation for this transform
    /*public Matrix3x2 GetGlobalTransform() {
        using var _ = new ProfilingScope("Global Transform Calculation");
        // Start with the local transformation matrix
        // Start with the local transformation matrix
        // Adjust the pivot by moving to the pivot, rotating/scaling, then moving back

        // Pivot will be 0-1, so we need to use `GetBounds` to get the actual pivot point
        var pivot = Bounds.Size * Pivot;


        var pivotTranslation = Matrix3x2.CreateTranslation(-pivot);
        var rotationMatrix   = Matrix3x2.CreateRotation(MathF.PI / 180 * Rotation);
        var scaleMatrix      = Matrix3x2.CreateScale(Scale);
        var finalTranslation = Matrix3x2.CreateTranslation(pivot + Position);

        // Combine transformations in the correct order
        var localTransform = pivotTranslation * scaleMatrix * rotationMatrix * finalTranslation;

        // If there's no parent, this is the global transform
        if (Parent == null) {
            cachedGlobalTransform = localTransform;
        } else {
            cachedGlobalTransform = localTransform * Parent.GetGlobalTransform();
        }

        return cachedGlobalTransform;
    }*/

    public Vector2 GlobalPosition {
        get {
            return cachedGlobalPosition;

            /*if (IsDirty) {
                cachedGlobalPosition = Vector2.Transform(Vector2.Zero, GetGlobalTransform());
            }*/
            // return cachedGlobalPosition = Vector2.Transform(Vector2.Zero, GetGlobalTransform());
        }
    }

    public Vector2 GlobalScale {
        get {
            /*if (!IsDirty)
                return cachedGlobalScale;

            cachedGlobalScale = Scale;
            var currentParent = Parent;
            while (currentParent != null) {
                cachedGlobalScale *= currentParent.Scale;
                currentParent     =  currentParent.Parent;
            }
            return cachedGlobalScale;*/
            return cachedGlobalScale;

            // if (Parent != null) {
            // return Scale * Parent.GlobalScale;
            // }
            // return Scale;
        }
    }


    public float GlobalRotation {
        get {
            /*if (!IsDirty)
                return cachedGlobalRotation;

            cachedGlobalRotation = Rotation;
            var currentParent = Parent;
            while (currentParent != null) {
                cachedGlobalRotation += currentParent.Rotation;
                currentParent        =  currentParent.Parent;
            }
            return cachedGlobalRotation;*/

            return cachedGlobalRotation;

            // if (Parent != null) {
            // return Rotation + Parent.GlobalRotation;
            // }
            // return Rotation;
        }
    }

    public Vector2 Forward {
        get { return Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation(MathF.PI / 180 * Rotation)); }
    }

    public Vector2 Right {
        get { return Vector2.Transform(Vector2.UnitY, Matrix3x2.CreateRotation(MathF.PI / 180 * Rotation)); }
    }

    public override string ToString() {
        return $"Position: {Position}, Rotation: {Rotation}, Scale: {Scale}";
    }

    public Common.Mathematics.Rectangle Bounds {
        get {
            // using var _ = new ProfilingScope("Bounds Calculation");

            var bounds = new Common.Mathematics.Rectangle(0, 0, 0, 0);

            if (Owner != null) {
                foreach (var p in Owner.ComponentsImplementing<IBoundsProvider>()) {
                    var componentBounds = p.Bounds;
                    bounds = bounds.Union(componentBounds);
                }

                // Now we get the child bounds
                foreach (var child in Owner.Children) {
                    bounds = bounds.Union(child.Transform2D.Bounds);
                }
            }


            bounds.Left = (int) Position.X;
            bounds.Top  = (int) Position.Y;

            return bounds;
        }
    }

    public void DrawDebug(GameTime time, Camera2D camera) {
        // draw a point for the pivot
        // var bounds = Bounds;
        // var pivot  = bounds.Position + (bounds.Size * Pivot);
        // Graphics.DrawRectangleLines((int) bounds.X, (int) bounds.Y, (int) bounds.Width, (int) bounds.Height, Color.Green);
        // Graphics.DrawCircleV(pivot, 5, Color.Red);
    }
}