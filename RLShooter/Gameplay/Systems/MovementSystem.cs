using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using RLShooter.Gameplay.Components;

namespace RLShooter.Gameplay.Systems;

public class MovementSystem : BaseSystem<World, float>, ISceneECSSystem {

    public MovementSystem(World world) : base(world) { }

    private QueryDescription Move_QueryDescription = new() {
        All       = [typeof(Position)],
        Any       = [],
        None      = [],
        Exclusive = []
    };

    private World _Move_Initialized;
    private Query _Move_Query;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void MoveQuery(World world, float time) {
        if (!ReferenceEquals(_Move_Initialized, world)) {
            _Move_Query       = world.Query(in Move_QueryDescription);
            _Move_Initialized = world;
        }

        var job = new MoveQueryJobChunk {time = time, world = world};
        world.InlineParallelChunkQuery(in Move_QueryDescription, job);
    }

    private struct MoveQueryJobChunk : IChunkJob {
        public float time;
        public World world;
        public void Execute(ref Chunk chunk) {
            var     chunkSize          = chunk.Size;
            ref var entityFirstElement = ref chunk.Entity(0);

            ref var positionFirstElement = ref chunk.GetFirst<Position>();
            foreach (var entityIndex in chunk) {
                ref readonly var entity = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                ref var          pos    = ref Unsafe.Add(ref positionFirstElement, entityIndex);
                Move(time, world, entity, ref pos);
            }
        }
    }

    private static int w;
    private static int h;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Update(in float data) {
        w = GetScreenWidth();
        h = GetScreenHeight();

        // MoveQuery(World, data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Move(
        [Data] float time,
        World        world,
        in  Entity   entity,
        ref Position pos
    ) {
        pos.Global.X += time * pos.Velocity.X;
        pos.Global.Y += time * pos.Velocity.Y;

        pos.LifeTime += time;

        // increase our scale by 0.1 every second
        // pos.Scale += new Vector2(1, 1) * time;

        if (pos.VelocityDecayFactor > 0) {
            var decayFactor = MathF.Exp(-pos.VelocityDecayFactor * time); // Using e^(-k * t) for exponential decay.
            pos.Velocity *= decayFactor;
        }

        var vel = pos.Velocity.LengthSquared();
        if (vel < pos.MinVelocityToDestroy * pos.MinVelocityToDestroy) {
            pos.Velocity = Vector2.Zero;
            if (pos.DestroyObjectOnZeroVelocity) {
                world.Destroy(entity);
            }
        }
    }

}