using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.System;
using Box2D.NetStandard.Dynamics.Bodies;
using RLShooter.App.Profiling;
using RLShooter.Gameplay.Components;
using RLShooter.GameScene;
using RLShooter.GameScene.Physics;
using World = Arch.Core.World;

namespace RLShooter.Gameplay.Systems;

public class PhysicsSystem : BaseSystem<World, float>, ISceneECSSystem {
    private PhysicsWorld physicsWorld;

    private QueryDescription MainQuery = new QueryDescription().WithAll<Position, Body>();

    private World _UpdatePhysicsBodies_Initialized;
    private Query _UpdatePhysicsBodies_Query;

    public PhysicsSystem(World world, PhysicsWorld pWorld) : base(world) {
        physicsWorld = pWorld;
        
        // Time.FixedUpdate += FixedUpdate;
    }
    private void FixedUpdate(float t) {
        using var _ = new ProfilingScope("PhysicsSystem.Update");

        UpdatePhysicsBodiesQuery(World, t, true, false);

        using(new ProfilingScope("PhysicsSystem.Step")) {
            physicsWorld.Step(t, 2, 2);
        }

        UpdatePhysicsBodiesQuery(World, t, false, true);
        
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdatePhysicsBodiesQuery(World world, float time, bool preStep = false, bool postStep = false) {
        
        using var _ = new ProfilingScope("PhysicsSystem.UpdateBodies(" + (preStep ? "pre" : "post") + "Step)");
        
        if (!ReferenceEquals(_UpdatePhysicsBodies_Initialized, world)) {
            _UpdatePhysicsBodies_Query       = world.Query(in MainQuery);
            _UpdatePhysicsBodies_Initialized = world;
        }

        foreach (ref var chunk in _UpdatePhysicsBodies_Query.GetChunkIterator()) {
            var     chunkSize            = chunk.Size;
            ref var entityFirstElement   = ref chunk.Entity(0);
            ref var positionFirstElement = ref chunk.GetFirst<Position>();
            ref var bodyFirstElement     = ref chunk.GetFirst<Body>();
            foreach (var entityIndex in chunk) {
                ref readonly var entity   = ref Unsafe.Add(ref entityFirstElement, entityIndex);
                ref var          position = ref Unsafe.Add(ref positionFirstElement, entityIndex);
                ref var          body     = ref Unsafe.Add(ref bodyFirstElement, entityIndex);

                if (preStep) {
                    var vel = position.Velocity;
                    if(float.IsNaN(vel.X) || float.IsNaN(vel.Y)) {
                        vel = Vector2.Zero;
                    }
                    body.SetLinearVelocity(vel / PhysicsConstants.PhysicsToPixelsRatio);
                }

                if (postStep) {
                    var pos = body.GetPosition();
                    position.Global = new Vector2(pos.X, pos.Y) * PhysicsConstants.PhysicsToPixelsRatio;
                }
                
            }
        }
    }

    public override void Update(in float t) {
        base.Update(in t);
        
        
    }


}