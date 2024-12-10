using Arch.System;
using DryIoc;
using RLShooter.App;
using RLShooter.Utils.Collections;

namespace RLShooter.Gameplay.Systems;

public interface ISceneECSSystem : ISystem<float> { }
public interface ISceneRenderSystem : ISystem<float> { }

public interface ISceneRenderUISystem : ISceneRenderSystem {
    public void RenderUI(float delta);
}