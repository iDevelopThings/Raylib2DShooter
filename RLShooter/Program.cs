using Box2D.NetStandard.Dynamics.Bodies;
using Hexa.NET.ImGui;
using RLShooter.App;
using RLShooter.App.Assets;
using RLShooter.App.InputManagement;
using RLShooter.App.Profiling;
using RLShooter.Common.Utils;
using RLShooter.Gameplay.Components;
using RLShooter.Gameplay.Systems;
using RLShooter.GameScene;
using RLShooter.GameScene.Physics;
using MouseButton = Hexa.NET.Raylib.MouseButton;

Application.Init();

var scene = Application.CreateScene("Root", true);

CameraComponent.Create(scene);

var bulletSprite = AssetManager.LoadAsset<Texture>("Resources/Bullet.png");

var playerRef = PlayerControlSystem.CreatePlayer(scene);

Time.SetTargetFPS(0);

var rng = new Random();


void CreateBullet() {
    var ps       = PlayerControlSystem.PlayerState;
    var velSpeed = 10_000;
    var velDir   = ps.AimDirection.Normalize() * velSpeed;

    // Add some randomness to the bullet direction, within a cone
    var angle = MathF.Atan2(velDir.Y, velDir.X);
    angle  += rng.NextSingle() * MathF.PI / 16 - MathF.PI / 32;
    velDir =  new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * velSpeed;

    var bullet = scene.CreateEntity("Bullet")
           .Add(new Position(
                    ps.Muzzle,
                    // ps.AimDirection.Normalize() * 2000,
                    velDir,
                    Vector2.One * 1.5f
                ) {
                    DestroyObjectOnZeroVelocity = true,
                    MinVelocityToDestroy        = 100,
                    VelocityDecayFactor         = 0.55f,
                })
           .Add(new SpriteRenderable {
                Texture = bulletSprite,
                Pivot   = new Vector2(0.5f, 0.5f),
                Tint = new Color(
                    (byte) GetRandomValue(50, 240),
                    (byte) GetRandomValue(80, 240),
                    (byte) GetRandomValue(100, 240),
                    255
                ),
            })
           .Add(new DestroyAfterTime(5))
           .Add<Projectile>()
        ;
    
    PhysicsManager.EnqueueBodyCreation(new QueuedBodyCreation {
        BodyCreationFunc = () => {
            var bodyDef = BodyFactory.CreateBodyDef(d => {
                d.position = ps.Muzzle / PhysicsConstants.PhysicsToPixelsRatio;
                d.bullet   = true;
            });

            var bulletBody = BodyFactory.CreateCircularBody(
                bullet,
                bulletSprite.Asset.Width,
                Application.CurrentScene.PhysicsWorld,
                bodyDef,
                1
            );

            bulletBody.SetLinearVelocity(velDir / PhysicsConstants.PhysicsToPixelsRatio);

            return bulletBody;
        },
        EntityRef = bullet,
    });

}

Input.RegisterAction("Fire")
   .AddTrigger(new PressTrigger())
   .AddBinding(
        new MouseKeyboardGroup(
            false,
            new MouseKeyBinding(MouseButton.Left),
            new KeyboardKeyBinding(KeyboardKey.Space)
        )
    )
   .AddOnTriggered(action => {
        for (var i = 0; i < 50; i++) {
            CreateBullet();
        }
    });

while (!Application.ShouldQuit) {
    var time = Time.GetGameTime();

    using var frameScope = new ProfilingScope("Frame");

    Application.OnTick(time);

    scene.GameLoop(time);


    Time.FrameUpdate();
    Time.FixedUpdateTick();

}

CloseWindow();


if (!ImGui.GetCurrentContext().IsNull)
    ImGui.SaveIniSettingsToDisk("imgui.ini");