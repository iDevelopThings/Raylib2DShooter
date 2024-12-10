namespace RLShooter.Utils;


public struct Mode3DScope : IDisposable {

    /// <summary>
    /// Begin drawing in 3D mode.
    /// </summary>
    /// <param name="camera">Camera to use for drawing.</param>
    public Mode3DScope(Camera3D camera) {
        BeginMode3D(camera);
    }

    /// <summary>
    /// End drawing in 3D mode.
    /// </summary>
    public void Dispose() {
        EndMode3D();
    }
}
public struct Mode2DScope : IDisposable {

    public Mode2DScope(Camera2D camera) {
        BeginMode2D(camera);
    }

    /// <summary>
    /// End drawing in 3D mode.
    /// </summary>
    public void Dispose() {
        EndMode2D();
    }
}

public struct DrawScope : IDisposable {

    /// <summary>
    /// Begin drawing.
    /// </summary>
    public DrawScope() {
        BeginDrawing();
    }
    public DrawScope(Color color) {
        BeginDrawing();
        ClearBackground(color);
    }

    /// <summary>
    /// End drawing.
    /// </summary>
    public void Dispose() {
        EndDrawing();
    }
}