using System.Runtime.CompilerServices;

namespace RLShooter.Gameplay.Components;

public struct Position {
    public Vector2 Global;
    public Vector2 Velocity;

    public Vector2 GlobalScale = Vector2.One;
    public Vector2 Scale       = Vector2.One;

    public float GlobalRotation = 0;
    public float Rotation       = 0;

    public float LifeTime = 0;

    private Matrix3x2 _globalTransform;
    private bool      _hasGlobalTransform;
    public Matrix3x2 GlobalTransform {
        get {
            if (!_hasGlobalTransform) {
                return GetGlobalTransform();
            }
            return _globalTransform;
        }
        set {
            _globalTransform    = value;
            _hasGlobalTransform = true;
        }
    }
    
    public float VelocityDecayFactor         = 0f;
    public bool  DestroyObjectOnZeroVelocity = false;
    public float MinVelocityToDestroy        = 0.001f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Matrix3x2 GetGlobalTransform() {
        return
            Matrix3x2.CreateScale(Scale) *
            Matrix3x2.CreateRotation(MathF.PI / 180 * Rotation) *
            Matrix3x2.CreateTranslation(Global);
    }
    
    public Position(
        Vector2 global,
        Vector2 velocity
    ) {
        Global   = global;
        Velocity = velocity;
        Scale    = Vector2.One;
    }
    public Position(
        Vector2 global,
        Vector2 velocity,
        Vector2 scale
    ) : this(global, velocity) {
        Scale = scale;
    }
    public Position(
        Vector2 global,
        Vector2 velocity,
        Vector2 scale,
        float   rotation
    ) : this(global, velocity, scale) {
        Rotation = rotation;
    }

    public static Position Zero() => new(Vector2.Zero, Vector2.Zero);

    public Vector2 Forward =>
        Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation(MathF.PI / 180 * Rotation));

    public Vector2 Right =>
        Vector2.Transform(Vector2.UnitY, Matrix3x2.CreateRotation(MathF.PI / 180 * Rotation));

}