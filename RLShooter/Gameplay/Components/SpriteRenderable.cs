namespace RLShooter.Gameplay.Components;

public struct SpriteRenderable {
    public Texture Texture;
    public Vector2 Pivot = new(0.0f, 0.0f);
    public Color   Tint  = RayWhite;

    public int     Width      => Texture.Width;
    public int     Height     => Texture.Height;
    public Vector2 Dimensions => new(Width, Height);

    public SpriteRenderable() {
        Texture = default;
        Tint    = RayWhite;
    }

    public SpriteRenderable(Texture texture) {
        Texture = texture;
        Tint    = RayWhite;
    }
    public SpriteRenderable(Texture texture, Vector2 pivot) : this(texture) {
        Pivot = pivot;
    }
    public SpriteRenderable(Texture texture, Vector2 pivot, Color tint) : this(texture, pivot) {
        Tint = tint;
    }
}

public struct SolidSpriteBoxRender {
    public Color   Color = RayWhite;
    public Vector2 Size  = new(10, 10);

    public SolidSpriteBoxRender() {
        Color = RayWhite;
    }
}