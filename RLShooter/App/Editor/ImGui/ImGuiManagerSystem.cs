using Arch.Core;
using Arch.System;
using RLShooter.Config;
using RLShooter.Gameplay.Systems;

namespace RLShooter.App.Editor.ImGuiIntegration;

public class ImGuiManagerSystem : BaseSystem<World, float>, ISceneRenderUISystem {
    public ImGuiManagerSystem(World world) : base(world) { }

    public override void Initialize() {
        base.Initialize();
        
        if (AppConfig.IsEditorUiEnabled) {
            EditorWindowManager.Instance.Initialize();
        }
    }
    
    public override void BeforeUpdate(in float t) {
        base.BeforeUpdate(t);

        if (AppConfig.IsEditorUiEnabled) {
            EditorWindowManager.Instance.imGui.NewFrame();
        }
    }
    
    public override void Update(in float t) {
        base.Update(t);
        
        if (AppConfig.IsEditorUiEnabled) {
            EditorWindowManager.Instance.Update(t);
        }
    }
    
    public override void AfterUpdate(in float t) {
        base.AfterUpdate(t);

        if (AppConfig.IsEditorUiEnabled) {
            EditorWindowManager.Instance.imGui.EndFrame();
        }
    }
    public void RenderUI(float delta) {
        if (AppConfig.IsEditorUiEnabled) {
            EditorWindowManager.Instance.RenderFrame();
        }
    }

}