using Box2D.NetStandard.Dynamics.Bodies;
using fennecs;
using RLShooter.App;
using RLShooter.App.Assets;
using RLShooter.Gameplay.Components;
using RLShooter.GameScene;
using RLShooter.GameScene.Physics;

namespace RLShooter.Gameplay.Systems;

public struct PlayerControlComponent {
    public float MouseSensitivity    { get; set; } = 1f;
    public float MoveSpeed           { get; set; } = 1500.0f;
    public float SprintMovementScale { get; set; } = 1.5f;

    public Vector2 AimDirection { get; set; }
    public Vector2 Muzzle       { get; set; }

    public PlayerControlComponent() { }
}

public struct ParentOf {
    public Vector2 Offset { get; set; }

    public ParentOf(Vector2 offset) {
        Offset = offset;
    }
}

public partial class PlayerControlSystem : BaseSystem, ISceneECSSystem {

    public PlayerControlSystem(World world) : base(world) { }

    public static Entity                 Player      { get; set; }
    public static PlayerControlComponent PlayerState => Player.Get<PlayerControlComponent>(default).First();

    public static Entity CreatePlayer(Scene scene) {

        var orangeCharAssets = AssetManager.LoadAssets<Texture>("Resources/Character/Orange");
        var weaponAssets     = AssetManager.LoadAssets<Texture>("Resources/Weapons");


        var bodyDef = BodyFactory.CreateBodyDef(b => {
            b.position      = new Vector2(250, 250) / PhysicsConstants.PhysicsToPixelsRatio;
            b.fixedRotation = true;
        });

        var entity = scene.CreateEntity("Player")
           .Add<PlayerControlComponent>()
           .Add(
                new Position(
                    new Vector2(250, 250),
                    Vector2.Zero,
                    Vector2.One
                )
            );

        entity.Add(
            BodyFactory.CreateCircularBody(
                entity,
                orangeCharAssets["Head_Orange"].Asset.Width,
                Application.CurrentScene.PhysicsWorld,
                bodyDef,
                99
            )
        );

        var backpack = scene.CreateEntity("Backpack")
           .Add<Position>(Position.Zero())
           .Add(new SpriteRenderable(orangeCharAssets["Backpack_LG_Orange"]));

        var armL = scene.CreateEntity("Arm_L")
           .Add<Position>(Position.Zero())
           .Add(new SpriteRenderable(orangeCharAssets["Arm_L_Orange"]));

        var armR = scene.CreateEntity("Arm_R")
           .Add<Position>(Position.Zero())
           .Add(new SpriteRenderable(orangeCharAssets["Arm_R_Orange"]));

        var weapon = scene.CreateEntity("Weapon")
           .Add<Position>(Position.Zero())
           .Add(new SpriteRenderable(weaponAssets["AR"], new Vector2(0.5f, 0.5f)));

        var head = scene.CreateEntity("Head")
           .Add<Position>(Position.Zero())
           .Add(new SpriteRenderable(orangeCharAssets["Head_Orange"]));

        entity.Add(new ParentOf(new Vector2(-11, -72)), backpack);
        entity.Add(new ParentOf(new Vector2(-51, 24)), armL);
        entity.Add(new ParentOf(new Vector2(78, 24)), armR);
        entity.Add(new ParentOf(new Vector2(68, 224)), weapon);
        entity.Add(new ParentOf(new Vector2(0, 0)), head);

        return Player = entity;
    }

    public void UpdatePlayer(
        float                      time,
        in  Entity                 entity,
        ref PlayerControlComponent player,
        ref Position               pos,
        ref Body                   body
    ) {
        ref var camera = ref CameraComponent.Handle.Ref();

        var mousePosition = GetScreenToWorld2D(GetMousePosition(), camera.Camera);

        player.AimDirection = new Vector2(
            mousePosition.X - pos.Global.X,
            mousePosition.Y - pos.Global.Y
        );

        // Calculate the angle in radians from the direction vector
        var rotation = (float) Math.Atan2(player.AimDirection.Y, player.AimDirection.X) * (180.0f / (float) Math.PI);
        rotation -= 90;
        rotation =  rotation < 0 ? 360 + rotation : rotation;

        pos.Rotation = rotation;

        var movementInput = Vector2.Zero;

        if (IsKeyDown((int) KeyboardKey.W)) movementInput.X += 1;
        if (IsKeyDown((int) KeyboardKey.S)) movementInput.X -= 1;
        if (IsKeyDown((int) KeyboardKey.A)) movementInput.Y -= 1;
        if (IsKeyDown((int) KeyboardKey.D)) movementInput.Y += 1;

        var movement = pos.Forward * movementInput.Y + pos.Right * movementInput.X;
        if (movement != Vector2.Zero)
            movement = Vector2.Normalize(movement);

        pos.Global += movement * player.MoveSpeed * time;
        body.SetTransform(pos.Global / PhysicsConstants.PhysicsToPixelsRatio, pos.Rotation * MathF.PI / 180);

        player.Muzzle = pos.Global + pos.Forward * 20;

        var parentPos = pos;
        World.Query<ParentOf>(entity)
           .Stream()
           .For((in Entity child, ref ParentOf relation) => {
                ref var childPos = ref child.Ref<Position>();


                var localTransform = Matrix3x2.CreateScale(childPos.Scale) *
                                     Matrix3x2.CreateRotation(MathF.PI / 180 * childPos.Rotation) *
                                     Matrix3x2.CreateTranslation(relation.Offset);

                var globalTransform = localTransform * parentPos.GetGlobalTransform();

                childPos.GlobalTransform = globalTransform;
                childPos.Global          = Vector2.Transform(Vector2.Zero, globalTransform);
                childPos.GlobalRotation  = parentPos.Rotation + childPos.Rotation;
                childPos.GlobalScale     = parentPos.Scale * childPos.Scale;
            });


        camera.TargetPosition = pos.Global;
        camera.Update(time);
    }

    public override void Update(in float t) {
        base.Update(in t);

        if (!Player.Alive) return;

        var q = World.Query<PlayerControlComponent, Position, Body>()
           .Stream();

        var f = t;
        q.For((in Entity entity, ref PlayerControlComponent player, ref Position pos, ref Body body) => {
            UpdatePlayer(f, entity, ref player, ref pos, ref body);
        });

    }
}