using System.ComponentModel;
using System.Runtime.Serialization;

namespace RLShooter.Config;

public partial class AppConfig : ConfigBase<AppConfig> {
    [ConfigValue, DefaultValue(true)]
    private bool _fxaaShaderEnabled = true;

    [ConfigValue]
    private HashSet<string> _openEditorWindows = [];

    [ConfigValue, DefaultValue(true)]
    private bool _isEditorUiEnabled = true;

    [ConfigValue, DefaultValue("1280, 720")]
    private Vector2 _windowSize = new(1280, 720);

    [ConfigValue, DefaultValue("200, 200")]
    private Vector2 _windowPosition = new(200, 200);

    [ConfigValue, DefaultValue("00000000-0000-0000-0000-000000000000")]
    private Guid _lastSelectedObjectId = Guid.Empty;

    [ConfigValue, DefaultValue(1.0f)]
    private float _cameraZoom = 1.0f;

    [OnDeserialized]
    public void OnDeserializedMethod(StreamingContext context) {
        if (_openEditorWindows == null) {
            _openEditorWindows = [];
        }

    }
}