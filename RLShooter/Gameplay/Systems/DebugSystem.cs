using Arch.Core;
using Arch.Core.Utils;
using Arch.System;
using RLShooter.App;
using RLShooter.Gameplay.Components;

namespace RLShooter.Gameplay.Systems;

public class DebugSystem : BaseSystemSingleton<DebugSystem>, ISceneRenderUISystem {

    public PhysicsWorld PhysicsWorld { get; set; }
    public DebugSystem(World world, PhysicsWorld pworld) : base(world) {
        PhysicsWorld = pworld;
    }

    private readonly QueryDescription _renderableQuery = new QueryDescription()
       .WithAll<SpriteRenderable>();

    private const int MAX_BATCH_ELEMENTS = 8192;

    public Queue<Action> RenderQueue { get; } = new();

    public static void EnqueueRender(Action renderAction) {
        Instance.RenderQueue.Enqueue(renderAction);
    }

    public override void BeforeUpdate(in float t) {
        base.BeforeUpdate(t);
    }
    public override void Update(in float t) {
        base.Update(t);

        // PhysicsWorld?.DrawDebugData();
        // while (RenderQueue.Count > 0) {
        //     RenderQueue.Dequeue()?.Invoke();
        // }
    }
    public override void AfterUpdate(in float t) {
        base.AfterUpdate(t);
    }


    public void RenderUI(float delta) {

        var entitiesCount = World.CountEntities(_renderableQuery);

        DrawRectangle(0, 0, GetScreenWidth(), 40, Black);
        DrawText($"bunnies: {entitiesCount}", 120, 10, 20, Green);
        DrawText($"batched draw calls: {1 + entitiesCount / MAX_BATCH_ELEMENTS}", 320, 10, 20, Maroon);

        DrawFPS(10, 10);


    }

}