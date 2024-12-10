using Arch.Core;
using Arch.Core.Extensions;
using Arch.Relationships;
using Arch.System;
using Arch.System.SourceGenerator;
using Box2D.NetStandard.Dynamics.Bodies;
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

public partial class PlayerControlSystem : BaseSystem<World, float>, ISceneECSSystem {

    public PlayerControlSystem(World world) : base(world) { }

    public static EntityReference        Player      { get; set; }
    public static PlayerControlComponent PlayerState => ((Entity) Player).Get<PlayerControlComponent>();

    public static EntityReference CreatePlayer(World world) {

        var orangeCharAssets = AssetManager.LoadAssets<Texture>("Resources/Character/Orange");
        var weaponAssets     = AssetManager.LoadAssets<Texture>("Resources/Weapons");


        var bodyDef = BodyFactory.CreateBodyDef(b => {
            b.position      = new Vector2(250, 250) / PhysicsConstants.PhysicsToPixelsRatio;
            b.fixedRotation = true;
        });

        var entity = world.Create<PlayerControlComponent, Position, Body>(
            new PlayerControlComponent(),
            new Position(
                new Vector2(250, 250),
                Vector2.Zero,
                Vector2.One
            )
        );
        entity.Set(
            BodyFactory.CreateCircularBody(
                entity,
                orangeCharAssets["Head_Orange"].Asset.Width,
                Application.CurrentScene.PhysicsWorld,
                bodyDef,
                99
            )
        );
        Scene.Current.InitializeEntity("Player", ref entity);
        
        var backpack = world.Create(
            Position.Zero(),
            new SpriteRenderable(orangeCharAssets["Backpack_LG_Orange"])
        );
        Scene.Current.InitializeEntity("Backpack", ref backpack);
        
        var armL = world.Create(
            Position.Zero(),
            new SpriteRenderable(orangeCharAssets["Arm_L_Orange"])
        );
        Scene.Current.InitializeEntity("Arm_L", ref armL);
        
        var armR = world.Create(
            Position.Zero(),
            new SpriteRenderable(orangeCharAssets["Arm_R_Orange"])
        );
        Scene.Current.InitializeEntity("Arm_R", ref armR);
        
        var weapon = world.Create(
            Position.Zero(),
            new SpriteRenderable(weaponAssets["AR"], new Vector2(0.5f, 0.5f))
        );
        Scene.Current.InitializeEntity("Weapon", ref weapon);
        
        var head = world.Create(
            Position.Zero(),
            new SpriteRenderable {
                Texture = orangeCharAssets["Head_Orange"],
            }
        );
        Scene.Current.InitializeEntity("Head", ref head);

        entity.AddRelationship(backpack, new ParentOf(new Vector2(-11, -72)));
        entity.AddRelationship(armL, new ParentOf(new Vector2(-51, 24)));
        entity.AddRelationship(armR, new ParentOf(new Vector2(78, 24)));
        entity.AddRelationship(weapon, new ParentOf(new Vector2(68, 224)));
        entity.AddRelationship(head, new ParentOf(new Vector2(0, 0)));

        return Player = entity.Reference();
    }

    [Query]
    [All<PlayerControlComponent, Position, Body>]
    public void UpdatePlayer(
        [Data] float                  time,
        in     Entity                 entity,
        ref    PlayerControlComponent player,
        ref    Position               pos,
        ref    Body                   body
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


        ref var parentOfRelation = ref entity.GetRelationships<ParentOf>();
        foreach (var child in parentOfRelation) {
            var childEntity = child.Key;
            var relation    = child.Value;

            ref var childPos = ref childEntity.Get<Position>();

            var localTransform = Matrix3x2.CreateScale(childPos.Scale) *
                                 Matrix3x2.CreateRotation(MathF.PI / 180 * childPos.Rotation) *
                                 Matrix3x2.CreateTranslation(relation.Offset);

            var globalTransform = localTransform * pos.GetGlobalTransform();

            childPos.GlobalTransform = globalTransform;
            childPos.Global          = Vector2.Transform(Vector2.Zero, globalTransform);
            childPos.GlobalRotation  = pos.Rotation + childPos.Rotation;
            childPos.GlobalScale     = pos.Scale * childPos.Scale;

        }

        camera.TargetPosition = pos.Global;
        camera.Update(time);
    }

}