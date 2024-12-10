using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Arch.System.SourceGenerator;
using RLShooter.App;
using RLShooter.App.Profiling;
using RLShooter.Gameplay.Components;

namespace RLShooter.Gameplay.Systems;

public partial class DestroyAfterTimeSystem : BaseSystem<World, float>, ISceneECSSystem {

    public DestroyAfterTimeSystem(World world) : base(world) { }

    private QueryDescription DestroyAfter_QueryDescription = new QueryDescription()
       .WithAll<DestroyAfterTime>();

    private World _DestroyAfter_Initialized;
    private Query _DestroyAfter_Query;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DestroyAfterQuery(World world, float time) {
        if (!ReferenceEquals(_DestroyAfter_Initialized, world)) {
            _DestroyAfter_Query       = world.Query(in DestroyAfter_QueryDescription);
            _DestroyAfter_Initialized = world;
        }
        
        world.Query(DestroyAfter_QueryDescription, (ref Entity entity, ref DestroyAfterTime state) => {
            DestroyAfter(time, ref entity, ref state);
        });

        // var job = new DestroyAfterQueryJobChunk {time = time, world = world};
        // world.InlineParallelChunkQuery(in DestroyAfter_QueryDescription, job);
    }

    /*private struct DestroyAfterQueryJobChunk : IChunkJob {
        public float time;
        public World world;
        public void Execute(ref Chunk chunk) {
            var     chunkSize          = chunk.Size;
            ref var entityFirstElement = ref chunk.Entity(0);

            ref var destroyAfterFirstElement = ref chunk.GetFirst<DestroyAfterTime>();

            foreach (var entityIndex in chunk) {
                ref readonly var entity       = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                ref var          destroyAfter = ref Unsafe.Add(ref destroyAfterFirstElement, entityIndex);

                DestroyAfter(time, world, entity, ref destroyAfter);
            }
        }
    }*/

    public override void Update(in float t) {
        base.Update(in t);

        DestroyAfterQuery(World, t);
    }

    [Profile("System.Update.DestroyAfterTime")]
    private void DestroyAfter(float time, ref Entity entity, ref DestroyAfterTime state) {
        state.Elapsed += time;
        if (state.Elapsed >= state.Time) {
            state.Destroyed = true;
            Application.CurrentScene.Destroy(entity.Reference());
        }
    }

    /*[Query]
    [All<DestroyAfterTime>()]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [Profile("System.Update.DestroyAfterTime")]
    public void UpdateDestroy(
        [Data] float            time,
        ref    DestroyAfterTime state,
        in     Entity           entity
    ) {

        state.Elapsed += time;
        if (state.Elapsed >= state.Time) {
            state.Destroyed = true;
            Application.CurrentScene.Destroy(entity);
        }

    }*/

}