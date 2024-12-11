﻿using DryIoc;
using RLShooter.App;
using RLShooter.Utils.Collections;

namespace RLShooter.Gameplay.Systems;

public interface ISceneECSSystem : ISystem { }
public interface ISceneRenderSystem : ISystem { }

public interface ISceneRenderUISystem : ISceneRenderSystem {
    public void RenderUI(float delta);
}