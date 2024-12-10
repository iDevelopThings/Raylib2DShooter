using System.Runtime.CompilerServices;
using RLShooter.App.Assets;
using RLShooter.App.Editor.Windows.Inspectors;

namespace RLShooter.GameScene.Components;

public interface IBoundsProvider {
    public Common.Mathematics.Rectangle Bounds { get; }
}

/*public class SpriteRendererComponent : Component, IBoundsProvider, IRender2D {
    public AssetHandle<Texture> Texture { get; set; }

    public Color Tint { get; set; } = RayWhite;

    [InspectorMinMax(0, 1)]
    public Vector2 Pivot { get; set; } = new(0, 0);


    public Common.Mathematics.Rectangle Bounds => new(
        0,
        0,
        Texture.Asset.Width,
        Texture.Asset.Height
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Render2D(GameTime time, Camera2D camera) {
        if (!Texture.IsValid()) {
            return;
        }
        var tex = Texture.Asset;

        var transform = Owner.Transform2D;
        var position  = transform.LightWeight ? transform.Position : transform.GlobalPosition;
        var rotation  = transform.LightWeight ? transform.Rotation : transform.GlobalRotation;
        var scale     = transform.LightWeight ? transform.Scale : transform.GlobalScale;

        var srcRect = new Rectangle(
            0, 0,
            tex.Width,
            tex.Height
        );
        var destRect = new Rectangle(
            position.X,
            position.Y,
            tex.Width * scale.X,
            tex.Height * scale.Y
        );

        // Our pivot value is 0-1, so we need to convert it?
        var pivotOffset = new Vector2(
            Pivot.X * tex.Width * scale.X,
            Pivot.Y * tex.Height * scale.Y
        );

        // DrawTexturePro(Texture2D texture, Rectangle source, Rectangle dest, Vector2 origin, float rotation, Color tint)
        DrawTexturePro(
            tex,
            srcRect,
            destRect,
            pivotOffset,
            rotation,
            Tint
        );

    }

    public override void Destroy() {
        base.Destroy();
    }

}*/