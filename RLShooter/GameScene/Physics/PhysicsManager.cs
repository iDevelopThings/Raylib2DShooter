using System.Collections.Concurrent;
using Box2D.NetStandard.Dynamics.Bodies;
using fennecs;
using RLShooter.App;
using RLShooter.Gameplay.Components;

namespace RLShooter.GameScene.Physics;

public struct QueuedBodyCreation {
    public Entity       EntityRef        { get; set; }
    public Func<Body>   BodyCreationFunc { get; set; }
    public Action<Body> OnBodyCreated    { get; set; }
}

[ContainerSingleton]
public class PhysicsManager : Singleton<PhysicsManager>, IDisposable {
    public Thread Thread    { get; set; }
    public bool   IsRunning { get; set; }

    private static readonly Lock PhysicsLock = new();

    const float timeStep = 1f / 60f;

    private Scene        Scene { get; set; }
    private PhysicsWorld World => Scene.PhysicsWorld;

    private static ConcurrentQueue<QueuedBodyCreation>         BodyCreationQueue = new();
    private static ConcurrentQueue<KeyValuePair<Entity, Body>> BodyDeletionQueue = new();

    public static void EnqueueBodyCreation(QueuedBodyCreation action)
        => BodyCreationQueue.Enqueue(action);

    public static void EnqueueBodyDeletion(Entity entityRef, Body body) {
        entityRef.Remove<Body>();

        BodyDeletionQueue.Enqueue(new KeyValuePair<Entity, Body>(entityRef, body));
    }

    public static void StartThread(Scene scene) {
        Instance.Scene = scene;

        Instance.Thread = new Thread(Instance.Update);
        Instance.Thread.Start();

        Instance.IsRunning = true;
    }

    private void Update() {
        while (IsRunning) {

            while (BodyDeletionQueue.TryDequeue(out var action)) {
                World.DestroyBody(action.Value);
            }

            lock (PhysicsLock) {
                var processed = new HashSet<Entity>();
                while (BodyCreationQueue.TryDequeue(out var action)) {
                    if (!processed.Add(action.EntityRef))
                        continue;

                    var body = action.BodyCreationFunc();
                    if (action.EntityRef.Alive && !action.EntityRef.Has<Body>()) {
                        action.EntityRef.Add<Body>(body);
                    }
                    
                    action.OnBodyCreated?.Invoke(body);
                }
            }

            var physicsEntitiesStream = Scene.World.Query<Position, Body>()
               .Has<Body>()
               .Stream();

            physicsEntitiesStream.For((in Entity entity, ref Position pos, ref Body body) => {
                if (body == null)
                    return;

                var vel = pos.Velocity;
                if (float.IsNaN(vel.X) || float.IsNaN(vel.Y)) {
                    vel = Vector2.Zero;
                }
                body.SetLinearVelocity(vel / PhysicsConstants.PhysicsToPixelsRatio);
            });

            lock (PhysicsLock) {
                World.Step(timeStep, 8, 3);
            }

            physicsEntitiesStream = Scene.World.Query<Position, Body>()
               .Has<Body>()
               .Stream();
            physicsEntitiesStream.For((in Entity entity, ref Position pos, ref Body body) => {
                if (body == null)
                    return;

                var bPos = body.GetPosition();
                pos.Global = new Vector2(bPos.X, bPos.Y) * PhysicsConstants.PhysicsToPixelsRatio;
            });

            Thread.Sleep(16);
        }
    }


    public void Dispose() { }
}