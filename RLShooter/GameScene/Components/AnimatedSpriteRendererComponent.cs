using RLShooter.App.Editor.Windows.Inspectors;
using RLShooter.Common.Utils.Reflection;

namespace RLShooter.GameScene.Components;

/*public class AnimatedSprite : IDisposable {
    public List<Texture> OriginalFrames = new();
    public List<Texture> Frames         = new();
    public float           FrameTime { get; set; }

    public AnimatedSprite(string[] imagePaths, float frameTime) {
        FrameTime = frameTime;
        LoadFrames(imagePaths);
        Frames.AddRange(OriginalFrames);
        // PreGenerateIntermediateFrames(2);
    }

    public static AnimatedSprite FromDirectory(string directory, float frameTime) {
        var paths = Directory.GetFiles(directory);
        return new AnimatedSprite(paths, frameTime);
    }

    public static AnimatedSprite FromDirectory(
        string       directory,
        string       searchPattern,
        SearchOption searchOption,
        float        frameTime
    ) {
        var paths = Directory.GetFiles(directory, searchPattern, searchOption);
        return new AnimatedSprite(paths, frameTime);
    }

    public void LoadFrames(string[] imagePaths) {
        foreach (var path in imagePaths) {
            OriginalFrames.Add(LoadTexture(path));
        }
    }

    private void PreGenerateIntermediateFrames(int intermediateFrameCount) {
        for (var i = 0; i < OriginalFrames.Count; i++) {
            var nextFrame = (i + 1) % OriginalFrames.Count;
            for (var j = 1; j <= intermediateFrameCount; j++) {
                var t                 = j / (float) (intermediateFrameCount + 1);
                var interpolatedFrame = GenerateInterpolatedFrame(OriginalFrames[i], OriginalFrames[nextFrame], t);
                Frames.Add(interpolatedFrame);
            }
            // Add the current frame to the preGenerated list
            Frames.Add(OriginalFrames[i]);
        }
    }
    private unsafe Texture GenerateInterpolatedFrame(Texture frame1, Texture frame2, float t) {
        // Lock textures for reading
        var image1 = LoadImageFromTexture(frame1);
        var image2 = LoadImageFromTexture(frame2);

        var width  = image1.Width;
        var height = image1.Height;

        var interpolatedImage = GenImageColor(width, height, Blank);

        var pixels1 = new ReadOnlySpan<Color>(LoadImageColors(image1), image1.Width * image1.Height).ToArray();
        var pixels2 = new ReadOnlySpan<Color>(LoadImageColors(image2), image2.Width * image2.Height).ToArray();
            
        var interpolatedPixels = new Color[width * height];

        for (var i = 0; i < pixels1.Length; i++) {
            interpolatedPixels[i] = InterpolateColor(pixels1[i], pixels2[i], t);
        }

        // Update image with interpolated pixel data
        for (var y = 0; y < height; y++) {
            for (var x = 0; x < width; x++) {
                ImageDrawPixel(ref interpolatedImage, x, y, interpolatedPixels[y * width + x]);
            }
        }

        UnloadImage(image1);
        UnloadImage(image2);

        // Create texture from interpolated image
        var interpolatedTexture = LoadTextureFromImage(interpolatedImage);
        UnloadImage(interpolatedImage);

        return interpolatedTexture;
    }

    private Color InterpolateColor(Color color1, Color color2, float t) {
        var r = (byte) (color1.R + (color2.R - color1.R) * t);
        var g = (byte) (color1.G + (color2.G - color1.G) * t);
        var b = (byte) (color1.B + (color2.B - color1.B) * t);
        var a = (byte) (color1.A + (color2.A - color1.A) * t);
        return new Color(r, g, b, a);
    }
    public void Dispose() {
        foreach (var frame in OriginalFrames) {
            UnloadTexture(frame);
        }
    }
}

public class AnimatedSpriteRendererComponent : Component, IRenderUI {


    public int   _currentFrame  { get; set; }
    public float _timer         { get; set; }
    public float _speedModifier { get; set; } = 1.0f;
    public bool  _isPlaying     { get; set; } = true;

    public AnimatedSprite AnimatedSprite { get; set; }

    [HideInInspector]
    public List<Texture> Frames => AnimatedSprite?.Frames;
    [HideInInspector]
    public Texture CurrentFrame => Frames[_currentFrame];

    private float _frameTime = -1f;
    public float FrameTime {
        get {
            if (_frameTime == -1f) {
                return AnimatedSprite?.FrameTime ?? 0.1f;
            }
            return _frameTime;
        }
        set => _frameTime = value;
    }

    public Color Color = RayWhite;

    public AnimatedSpriteRendererComponent() { }
    public AnimatedSpriteRendererComponent(Vector2 size, Color color) : base() {
        this.Color = color;
    }

    public bool IsValid() => AnimatedSprite != null && Frames.Count > 0;

    public override void Update(GameTime time) {
        base.Update(time);

        if (!_isPlaying)
            return;
        if (!IsValid())
            return;

        _timer += time.Delta * _speedModifier;
        if (!(_timer >= FrameTime))
            return;

        _timer        = 0;
        _currentFrame = (_currentFrame + 1) % Frames.Count;
    }

    public void RenderUI(GameTime time) {
        if (!IsValid())
            return;
        
        var transform = Owner.Transform2D;
        var position  = transform.GlobalPosition;
        var srcRect = new Rectangle(
            0, 0,
            CurrentFrame.Width,
            CurrentFrame.Height
        );
        var destRect = new Rectangle(
            position.X,
            position.Y,
            CurrentFrame.Width * transform.GlobalScale.X,
            CurrentFrame.Height * transform.GlobalScale.Y
        );

        DrawTexturePro(
            CurrentFrame,
            srcRect,
            destRect,
            new Vector2(0, 0),
            transform.Rotation,
            Color
        );
        
    }




    public void  SetSpeed(float speed) => _speedModifier = speed;
    public float GetSpeed()            => _speedModifier;

    public void  SetFrameTime(float frameTime) => FrameTime = frameTime;
    public float GetFrameTime()                => FrameTime;

    public void Play()  => _isPlaying = true;
    public void Pause() => _isPlaying = false;
    public void Restart() {
        _isPlaying    = true;
        _currentFrame = 0;
        _timer        = 0;
    }



}*/