using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Box2D.NetStandard.Dynamics.Bodies;
using DotTiled;
using DotTiled.Serialization;
using RLShooter.App;
using RLShooter.App.Profiling;
using RLShooter.Gameplay.Components;
using RLShooter.Gameplay.Systems;
using RLShooter.GameScene.Physics;
using RLShooter.Utils;
using Schedulers;

namespace RLShooter.GameScene;

public struct WorldObject { }

public class Scene : IDisposable {

    public string Name { get; set; }

    public  World               World        { get; }
    public  PhysicsWorld        PhysicsWorld { get; set; }
    private GameContactListener _gameContactListener;

    public JobScheduler JobScheduler     { get; set; }
    public Group<float> ECSSystems       { get; set; }
    public Group<float> ECSRenderSystems { get; set; }

    public Map Map { get; set; }

    private bool _awake = false;

    public static Scene Current { get; set; }

    private ConcurrentQueue<EntityReference> _entitiesToDestroy = new();


    public Scene(string name = "Untitled") {
        Name  = name;
        World = World.Create();
        AppContainer.RegisterInstance(World);

        PhysicsWorld = new PhysicsWorld(new Vector2(0, 0));
        AppContainer.RegisterInstance(PhysicsWorld);

        _gameContactListener = new GameContactListener();

        JobScheduler = new(
            new JobScheduler.Config {
                ThreadPrefixName     = "Game",
                StrictAllocationMode = false,
            }
        );
        World.SharedJobScheduler = JobScheduler;

        ECSSystems       = new Group<float>("GameSystems");
        ECSRenderSystems = new Group<float>("RenderSystems");

        PhysicsManager.StartThread(this);

        Time.FixedUpdate += OnFixedUpdate;

        // var loader = Loader.Default();
        // Map = loader.LoadMap("Resources/TiledMap/untitled.tmx");
        // Console.WriteLine(Map.Layers.Count);

    }

    public void OnInitialize() {
        var ecsSystems = AppContainer.ResolveMany<ISceneECSSystem>().ToList();
        foreach (var system in ecsSystems) {
            ECSSystems.Add(system);
            AppContainer.RegisterInstance(system);
        }
        var ecsRenderSystems = AppContainer.ResolveMany<ISceneRenderSystem>().ToList();
        foreach (var system in ecsRenderSystems) {
            ECSRenderSystems.Add(system);
            AppContainer.RegisterInstance(system);
        }

        ECSSystems.Initialize();
        ECSRenderSystems.Initialize();

        PhysicsWorld.SetContactListener(_gameContactListener);
        PhysicsWorld.SetContactFilter(new GameContactFilter());
        PhysicsWorld.SetDebugDraw(new GamePhysicsDebugDraw(ECSRenderSystems.Get<DebugSystem>()));
        PhysicsWorld.SetDestructionListener(new GamePhysicsDestructionListener(this));

        CreateWall();
    }

    public void InitializeEntity(string name, ref Entity entity) {
        if (!entity.Has<Named>())
            entity.Add<Named>(name);

        if (!entity.Has<EditorFlags>())
            entity.Add<EditorFlags>();

    }

    public void CreateWall() {
        var pos  = new Vector2(0, 0);
        var size = new Vector2(100, 1000);

        var entity = World.Create<WorldObject, SolidSpriteBoxRender, Position>(
            new WorldObject(),
            new SolidSpriteBoxRender() {Color = Black, Size = size},
            new Position(new Vector2(0, 0), Vector2.Zero, new Vector2(1, 1))
        );

        InitializeEntity("Wall", ref entity);

        PhysicsManager.EnqueueBodyCreation(new QueuedBodyCreation {
            BodyCreationFunc = () => {
                var def = BodyFactory.CreateBodyDef(d => {
                    d.position      = pos / PhysicsConstants.PhysicsToPixelsRatio;
                    d.type          = BodyType.Static;
                    d.fixedRotation = true;
                });
                var body = BodyFactory.CreateBoxBody(
                    pos,
                    size,
                    PhysicsWorld,
                    def
                );

                body.SetUserData(entity.Reference());

                return body;
            },
            EntityRef = entity.Reference(),
        });

    }

    public void OnAwake() {
        _awake = true;
    }

    public void OnFixedUpdate(float t) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GameLoop(GameTime time) {
        if (!_awake) {
            OnAwake();
            return;
        }

        ECSSystems.BeforeUpdate(time.Delta);
        // This starts the IMGUI frame
        ECSRenderSystems.BeforeUpdate(time.Delta);

        ECSSystems.Update(time.Delta);

        using (new DrawScope(SkyBlue)) {

            using (new Mode2DScope(CameraComponent.Handle.Value.Camera)) {
                DrawGrid(100, 100);

                ECSRenderSystems.Update(time.Delta);
            }


            foreach (var system in ECSRenderSystems.Find<ISceneRenderUISystem>()) {
                system.RenderUI(time.Delta);
            }

            ECSRenderSystems.AfterUpdate(time.Delta);
        }

        ECSSystems.AfterUpdate(time.Delta);

        while (_entitiesToDestroy.TryDequeue(out var entityRef)) {
            if (!entityRef.IsAlive())
                continue;
            if (entityRef.Entity.Has<Body>()) {
                PhysicsManager.EnqueueBodyDeletion(entityRef, entityRef.Entity.Get<Body>());
            }
            World.Destroy(entityRef);
        }
    }


    public void OnUnload() { }

    public void Dispose() {
        OnUnload();
    }

    /*public IEnumerable<GameObject> GetRange(GameObject start, GameObject end) {
        var collect = false;
        foreach (var current in GameObjects) {
            if (current == start || current == end) {
                if (!collect) {
                    collect = true;
                } else {
                    yield return current;
                    break;
                }
            }

            if (collect && current.DisplayedInEditor) {
                yield return current;
            }
        }
    }*/


    public override string ToString() {
        return Name;
    }

    [Profile("Scene.DestroyEntity")]
    public void Destroy(EntityReference entity) {
        if (entity.IsAlive())
            _entitiesToDestroy.Enqueue(entity);
    }

    public IEnumerable<object> GetRange(Entity start, Entity end) {
        var collect = false;

        var entityIterator = new Span<Entity>();
        World.GetEntities(new QueryDescription(), entityIterator, start.Id);
        var ents = entityIterator.ToArray();

        foreach (var current in ents) {
            if (current == start || current == end) {
                if (!collect) {
                    collect = true;
                } else {
                    yield return current.Reference();
                    break;
                }
            }

            if (collect) {
                yield return current.Reference();
            }
        }
    }
}