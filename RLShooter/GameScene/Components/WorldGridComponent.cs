using RLShooter.GameScene.Rendering;

namespace RLShooter.GameScene.Components;

/*public class WorldGridObject : GameObject {
    [RequiredComponent]
    public WorldGridComponent Grid { get; set; }

    public WorldGridObject(string name, GameObject parent = null, Scene scene = null) : base(name, parent, scene) { }
}*/

/*public class WorldGridComponent : Component {
    private EngineShader shader { get; set; }
    private int          cameraPosLoc;
    private int          maxDistanceLoc;
    private int          gridSizeLoc;
    private int          lineWidthLoc;

    public int   PlaneSize   { get; set; } = 150;
    public int   CellSize    { get; set; } = 1;
    public float MaxDistance { get; set; } = 50;
    public float LineWidth = 0.05f;

    public override void BeginPlay() {
        base.BeginPlay();

        shader = EngineShader.Load(
            "Resources/Shaders/glsl330/grid.vs",
            "Resources/Shaders/glsl330/grid.fs"
        );

        cameraPosLoc   = shader.GetLocation("cameraPos");
        maxDistanceLoc = shader.GetLocation("maxDistance");
        gridSizeLoc    = shader.GetLocation("gridSize");
        lineWidthLoc   = shader.GetLocation("lineWidth");
    }

    /*public override void Render(GameTime time, CameraComponent camera) {
        base.Render(time, camera);

        shader.SetValue(cameraPosLoc, camera.Camera.Position);
        shader.SetValue(maxDistanceLoc, MaxDistance);
        shader.SetValue(gridSizeLoc, CellSize);
        shader.SetValue(lineWidthLoc, LineWidth);

        shader.Begin();

        // Draw a large quad as the grid plane
        Vector3 center = new Vector3(0, 0, 0);
        // Vector2 planeSize = new Vector2(GridSize * 2, GridSize * 2);
        Vector2 planeSize = new Vector2(PlaneSize, PlaneSize);
        Graphics.DrawPlane(center, planeSize, Color.White);

        shader.End();
    }#1#
}*/