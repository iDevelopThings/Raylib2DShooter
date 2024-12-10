using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.Joints;
using Box2D.NetStandard.Dynamics.World.Callbacks;

namespace RLShooter.GameScene.Physics;

public class GamePhysicsDestructionListener(Scene scene) : DestructionListener {

    public override void SayGoodbye(Joint   joint)   { }
    public override void SayGoodbye(Fixture fixture) { }
}