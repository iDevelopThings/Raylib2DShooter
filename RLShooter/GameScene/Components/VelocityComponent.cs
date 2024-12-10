using RLShooter.App.Profiling;
using RLShooter.Common.Utils;

namespace RLShooter.GameScene.Components;

/*public class VelocityComponent : Component {
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    public float MinVelocityToDestroy { get; set; } = 0.001f;
    public float MaxSpeed             { get; set; } = 0;
    public float DecayRate            { get; set; } = 0;

    public bool DestroyObjectOnZeroVelocity { get; set; } = false;

    private Vector2 StartPosition { get; set; }
    public override void BeginPlay() {
        base.BeginPlay();
        StartPosition = Transform2D.Position;
    }


    public override void Update(GameTime time) {
        
        // using var _        = new ProfilingScope("Velocity Component");
        if (Velocity.IsEmpty()) {
            return;
        }

        var delta   = time.Delta;
        // var prevPos = Transform2D.Position;

        // Move the object based on our velocity.
        Transform2D.Position += Velocity * delta;

        // Exponential decay of velocity
        // The closer to 1, the slower the decay. Lower values mean quicker decay.
        var decayFactor = MathF.Exp(-DecayRate * delta); // Using e^(-k * t) for exponential decay.
        Velocity *= decayFactor;

        // Update rotation based direction of velocity.
        Transform2D.Rotation = StartPosition.AngleTo(Transform2D.Position);

        // If the velocity is very close to zero, set it to zero explicitly.
        var vel = Velocity.LengthSquared();
        if (vel < MinVelocityToDestroy * MinVelocityToDestroy) {
            Velocity = Vector2.Zero;
            if (DestroyObjectOnZeroVelocity) {
                Owner.Destroy();
            }
        }

    }
}*/