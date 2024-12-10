using Arch.Core;
using Arch.Core.Extensions;
using RLShooter.App;
using RLShooter.App.InputManagement;
using RLShooter.Common.ArchExtensions;
using RLShooter.Common.Mathematics;
using RLShooter.Config;

namespace RLShooter.Gameplay.Components;

public struct CameraComponent {
    public Camera2D Camera;

    public Vector2 TargetPosition = Vector2.Zero;
    public float   TargetZoom     = AppConfig.CameraZoom;

    public static ComponentHandle<CameraComponent> Handle = null;

    // private static EntityReference MainCamera;
    // public static Camera2D? Main => MainCamera.IsAlive() ? MainCamera.Entity.Get<CameraComponent>().Camera : null;
    // public static ref CameraComponent MainRef => ref MainCamera.Entity.Get<CameraComponent>();

    public CameraComponent() { }

    public void SetZoom(float zoom) {
        Camera.Zoom          = zoom;
        AppConfig.CameraZoom = zoom;
    }

    public static void Create(World world) {
        var entity = world.Create(
            new CameraComponent {
                Camera = new Camera2D {
                    Offset   = Vector2.Zero,
                    Target   = Vector2.Zero,
                    Rotation = 0,
                    Zoom     = AppConfig.CameraZoom,
                },
            }
        );

        Handle = entity.GetHandle<CameraComponent>();
    }

    public void Update(float time) {
        const float minSpeed        = 100;
        const float minEffectLength = 10;
        const float fractionSpeed   = 0.95f;

        Camera.Offset = Application.Window.WindowSize / 2;

        var diff   = TargetPosition - Camera.Target;
        var length = diff.Length();
        if (length > minEffectLength) {
            var speed = MathF.Max(fractionSpeed * length, minSpeed);
            Camera.Target += diff * (speed * time / length);
        }


        var mouseScrollDelta = Input.GetMouseWheel();
        if (mouseScrollDelta != 0) {
            TargetZoom += mouseScrollDelta * 0.005f;
            if (TargetZoom < 0f)
                TargetZoom = 0f;
        }

        // TargetPosition = Character.Transform2D.Position;
        SetZoom(MathUtil.Lerp(Camera.Zoom, TargetZoom, 10f * time));
    }

}