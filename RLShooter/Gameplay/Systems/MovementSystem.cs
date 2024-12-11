using System.Runtime.CompilerServices;
using fennecs;
using RLShooter.Gameplay.Components;

namespace RLShooter.Gameplay.Systems;

public class MovementSystem : BaseSystem, ISceneECSSystem {

    public MovementSystem(World world) : base(world) { }


}