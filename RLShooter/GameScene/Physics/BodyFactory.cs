using Arch.Core;
using Arch.Core.Extensions;
using Box2D.NetStandard.Collision.Shapes;
using Box2D.NetStandard.Dynamics.Bodies;
using Box2D.NetStandard.Dynamics.Fixtures;

namespace RLShooter.GameScene.Physics;

public class PhysicsConstants {
    public const float PhysicsToPixelsRatio = 50f;
}

public class PooledBodyDef : BodyDef, IDisposable {
    public void Return() {
        userData        = (object) null;
        position        = Vector2.Zero;
        angle           = 0.0f;
        linearVelocity  = Vector2.Zero;
        angularVelocity = 0.0f;
        linearDamping   = 0.0f;
        angularDamping  = 0.0f;
        allowSleep      = true;
        awake           = true;
        fixedRotation   = false;
        bullet          = false;
        type            = BodyType.Static;
        enabled         = true;
        gravityScale    = 1f;

        BodyFactory.BodyDefPool.Return(this);
    }
    public void Dispose() {
        Return();
    }
}

public struct Projectile { };

public class BodyFactory {

    public static ObjectPool<FixtureDef>    FixtureDefPool = new() {MaxItemsThreshold = 1000};
    public static ObjectPool<PooledBodyDef> BodyDefPool    = new() {MaxItemsThreshold = 1000};

    public static PooledBodyDef CreateBodyDef(Action<PooledBodyDef> ctor) {
        var def = BodyDefPool.Rent();
        ctor(def);
        return def;
    }

    public static Body CreateCircularBody(
        Entity        entity,
        int           width,
        PhysicsWorld  physicsWorld,
        PooledBodyDef bodyDef,
        float         density = 1f,
        bool          awake   = true
    ) {
        var fixture = FixtureDefPool.Rent();

        fixture.shape = new CircleShape() {
            Radius = width / 2f / PhysicsConstants.PhysicsToPixelsRatio,
        };
        fixture.density  = density;
        fixture.friction = 0.0f;
        // fixture.isSensor = density <= 1f;

        bodyDef.type = BodyType.Dynamic;

        var physicsBody = physicsWorld.CreateBody(bodyDef);
        physicsBody.CreateFixture(fixture);
        physicsBody.SetUserData(entity.Reference());
        physicsBody.SetAwake(awake);


        FixtureDefPool.Return(fixture);
        bodyDef.Return();

        return physicsBody;
    }
    public static Body CreateBoxBody(
        Vector2       position,
        Vector2       size,
        PhysicsWorld  physicsWorld,
        PooledBodyDef def
    ) {
        var fixture = FixtureDefPool.Rent();

        fixture.shape = new PolygonShape();

        ((PolygonShape) fixture.shape).SetAsBox(
            (size.X / 2f) / PhysicsConstants.PhysicsToPixelsRatio,
            (size.Y / 2f) / PhysicsConstants.PhysicsToPixelsRatio,
            // Center pos
            new Vector2((size.X / 2f) / PhysicsConstants.PhysicsToPixelsRatio, (size.Y / 2f) / PhysicsConstants.PhysicsToPixelsRatio),
            0.0f
        );
        fixture.density  = 1f;
        fixture.friction = 0.0f;
        // fixture.isSensor = true;

        def.position = position / PhysicsConstants.PhysicsToPixelsRatio;
        def.type     = BodyType.Static;

        var physicsBody = physicsWorld.CreateBody(def);
        physicsBody.CreateFixture(fixture);
        physicsBody.SetAwake(true);

        FixtureDefPool.Return(fixture);
        def.Return();

        return physicsBody;
    }
}