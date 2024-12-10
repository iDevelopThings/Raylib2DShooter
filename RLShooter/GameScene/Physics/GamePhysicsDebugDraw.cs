using Box2D.NetStandard.Common;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using RLShooter.Common.Utils;
using RLShooter.Gameplay.Systems;
using Color = Box2D.NetStandard.Dynamics.World.Color;
using Transform = Box2D.NetStandard.Common.Transform;

#pragma warning disable CS0618 // Type or member is obsolete

namespace RLShooter.GameScene.Physics;

public class GamePhysicsDebugDraw : DebugDraw {
    private DebugSystem d;

    public GamePhysicsDebugDraw(DebugSystem debugSystem) {
        d = debugSystem;
        AppendFlags(DrawFlags.Shape);
        AppendFlags(DrawFlags.Aabb);
    }

    public override void DrawTransform(in Transform xf) {
        throw new NotImplementedException();
    }
    public override void DrawPoint(in Vector2 position, float size, in Color color) {
        var gPos = position / PhysicsConstants.PhysicsToPixelsRatio;
        var c    = new Common.Mathematics.Color(color.R, color.G, color.B, color.A);
        d.RenderQueue.Enqueue(() => {
            DrawPixelV(gPos, c.ToRlColor());
        });
    }
    public override void DrawPolygon(in Vec2[] vertices, int vertexCount, in Color color) {
        var vec2s  = vertices;
        var color1 = color;
        d.RenderQueue.Enqueue(() => {
            for (int i = 0; i < vertexCount; i++) {
                var v1 = (Vector2) vec2s[i] / PhysicsConstants.PhysicsToPixelsRatio;
                var v2 = (Vector2) vec2s[(i + 1) % vertexCount] / PhysicsConstants.PhysicsToPixelsRatio;

                DrawLineV(v1, v2, new Common.Mathematics.Color(color1.R, color1.G, color1.B, color1.A).ToRlColor());
            }
        });
    }
    public override void DrawSolidPolygon(in Vec2[] vertices, int vertexCount, in Color color) {
        DrawPolygon(vertices, vertexCount, color);
    }
    public override void DrawCircle(in Vec2 center, float radius, in Color color) {
        var c = ((Vector2) center) / PhysicsConstants.PhysicsToPixelsRatio;
        var r = radius / PhysicsConstants.PhysicsToPixelsRatio;

        var color1 = color;
        d.RenderQueue.Enqueue(() => {
            DrawCircleV(
                c,
                r,
                new Common.Mathematics.Color(color1.R, color1.G, color1.B, color1.A).ToRlColor()
            );
        });
    }
    public override void DrawSolidCircle(in Vec2 center, float radius, in Vec2 axis, in Color color) {
        DrawCircle(center, radius, color);
    }
    public override void DrawSegment(in Vec2 p1, in Vec2 p2, in Color color) {
        throw new NotImplementedException();
    }
}