using Arch.Core;
using Arch.Core.Extensions;
using Box2D.NetStandard.Collision;
using Box2D.NetStandard.Dynamics.Contacts;
using Box2D.NetStandard.Dynamics.World;
using Box2D.NetStandard.Dynamics.World.Callbacks;
using System.Collections.Generic;

namespace RLShooter.GameScene.Physics;

public class GameContactListener : ContactListener {

    public override void BeginContact(in Contact contact) {
        if (contact.GetFixtureA().Body.UserData == null || contact.GetFixtureB().Body.UserData == null)
            return;
        
        var a = contact.GetFixtureA().Body.GetUserData<EntityReference>();
        var b = contact.GetFixtureB().Body.GetUserData<EntityReference>();

        Console.WriteLine("Collision started between " + a + " and " + b);
    }

    public override void EndContact(in Contact contact) {
        if (contact.GetFixtureA().Body.UserData == null || contact.GetFixtureB().Body.UserData == null)
            return;
        
        var a = contact.GetFixtureA().Body.GetUserData<EntityReference>();
        var b = contact.GetFixtureB().Body.GetUserData<EntityReference>();

        Console.WriteLine("Collision ended between " + a + " and " + b);
    }

    public override void PostSolve(in Contact contact, in ContactImpulse impulse) { }

    public override void PreSolve(in Contact contact, in Manifold oldManifold) { }
}