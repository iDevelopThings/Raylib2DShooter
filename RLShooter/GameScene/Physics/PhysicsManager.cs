﻿using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.World;
using RLShooter.App;
using Position = RLShooter.Gameplay.Components.Position;

namespace RLShooter.GameScene.Physics;

public struct QueuedBodyCreation {
    public EntityReference EntityRef        { get; set; }
    public Func<Body>      BodyCreationFunc { get; set; }
    public Action<Body>    OnBodyCreated    { get; set; }
}

[ContainerSingleton]
public class PhysicsManager : Singleton<PhysicsManager>, IDisposable {
    public Thread Thread    { get; set; }
    public bool   IsRunning { get; set; }

    private static readonly Lock PhysicsLock = new();

    const float timeStep = 1f / 60f;

    private Scene        Scene { get; set; }
    private PhysicsWorld World => Scene.PhysicsWorld;

    private static ConcurrentQueue<QueuedBodyCreation>                  BodyCreationQueue = new();
    private static ConcurrentQueue<KeyValuePair<EntityReference, Body>> BodyDeletionQueue = new();

    public static void EnqueueBodyCreation(QueuedBodyCreation action)
        => BodyCreationQueue.Enqueue(action);

    public static void EnqueueBodyDeletion(EntityReference entityRef, Body body) {
        entityRef.Entity.Remove<Body>();

        BodyDeletionQueue.Enqueue(new KeyValuePair<EntityReference, Body>(entityRef, body));
    }

    public static void StartThread(Scene scene) {
        Instance.Scene = scene;

        Instance.Thread = new Thread(Instance.Update);
        Instance.Thread.Start();

        Instance.IsRunning = true;
    }

    public struct VelocityUpdate : IForEach<Position, Body> {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Update(ref Position pos, ref Body body) { }
    }

    private QueryDescription queryDescription = new QueryDescription().WithAll<Position, Body>();

    private void Update() {
        while (IsRunning) {

            while (BodyDeletionQueue.TryDequeue(out var action)) {
                World.DestroyBody(action.Value);
            }

            lock (PhysicsLock) {
                while (BodyCreationQueue.TryDequeue(out var action)) {
                    var body = action.BodyCreationFunc();
                    if (action.EntityRef.IsAlive()) {
                        action.EntityRef.Entity.Add(body);
                    }
                    action.OnBodyCreated?.Invoke(body);
                }
            }

            Scene.World.Query(queryDescription, static (Entity entity, ref Position pos, ref Body body) => {
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

            Scene.World.Query(queryDescription, static (Entity entity, ref Position pos, ref Body body) => {
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