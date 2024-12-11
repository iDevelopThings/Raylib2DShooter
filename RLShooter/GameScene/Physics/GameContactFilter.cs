
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using fennecs;

namespace RLShooter.GameScene.Physics;

public class GameContactFilter : ContactFilter {

    public override bool ShouldCollide(Fixture fixtureA, Fixture fixtureB) {
        if(!base.ShouldCollide(fixtureA, fixtureB))
            return false;
        
        if (fixtureA.Body.UserData == null || fixtureB.Body.UserData == null)
            return false;

        
        var a = fixtureA.Body.GetUserData<Entity>();
        var b = fixtureB.Body.GetUserData<Entity>();
        
        if(!a.Alive || !b.Alive)
            return false;
        
        var isWorldObject = a.Has<WorldObject>() || b.Has<WorldObject>();
        var isProjectile  = a.Has<Projectile>() || b.Has<Projectile>();

        if (isWorldObject && isProjectile)
            return true;

        return false;
    }
}