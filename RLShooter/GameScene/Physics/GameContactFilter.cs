using Arch.Core;
using Arch.Core.Extensions;
using Box2D.NetStandard.Dynamics.Fixtures;
using Box2D.NetStandard.Dynamics.World.Callbacks;

namespace RLShooter.GameScene.Physics;

public class GameContactFilter : ContactFilter {

    public override bool ShouldCollide(Fixture fixtureA, Fixture fixtureB) {
        if(!base.ShouldCollide(fixtureA, fixtureB))
            return false;
        
        if (fixtureA.Body.UserData == null || fixtureB.Body.UserData == null)
            return false;

        
        var a = fixtureA.Body.GetUserData<EntityReference>();
        var b = fixtureB.Body.GetUserData<EntityReference>();
        
        if(!a.IsAlive() || !b.IsAlive())
            return false;
        
        var isWorldObject = a.Entity.Has<WorldObject>() || b.Entity.Has<WorldObject>();
        var isProjectile  = a.Entity.Has<Projectile>() || b.Entity.Has<Projectile>();

        if (isWorldObject && isProjectile)
            return true;

        return false;
    }
}