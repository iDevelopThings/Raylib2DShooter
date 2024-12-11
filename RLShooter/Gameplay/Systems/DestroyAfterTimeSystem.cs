using System.Runtime.CompilerServices;
using fennecs;
using RLShooter.App;
using RLShooter.App.Profiling;
using RLShooter.Gameplay.Components;
using RLShooter.GameScene;

namespace RLShooter.Gameplay.Systems;

public partial class DestroyAfterTimeSystem : BaseSystem, ISceneECSSystem {

    public DestroyAfterTimeSystem(World world) : base(world) { }

    public override void Update(in float t) {
        base.Update(in t);

        // DestroyAfterQuery(World, t);

        DestroyAll(t);
    }
    [Profile("System.Update.DestroyAfterTime")]
    private void DestroyAll(in float t) {
        var destroyable = World.Query<DestroyAfterTime>()
           .Stream();

        var f = t;
        destroyable.For((in Entity entity, ref DestroyAfterTime state) => {
            state.Elapsed += f;
            if (state.Elapsed >= state.Time) {
                state.Destroyed = true;
                entity.Add<QueuedForDestroy>();
                Scene.Current.Destroy(entity);
            }
        });
    }

}