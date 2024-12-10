
namespace RLShooter.GameScene.Rendering;

/*public class EngineShaderWatcher
{
    public static List<EngineShader> Shaders = new();

    static EngineShaderWatcher()
    {
        CoreEvents.OnTick += () =>
        {
            var toRemove = new List<EngineShader>();

            foreach (var shader in Shaders) {
                if (shader == null || shader.Shader.Id == 0) {
                    toRemove.Add(shader);
                    continue;
                }

                shader.UpdateHotReload();
            }

            foreach (var shader in toRemove) {
                Shaders.Remove(shader);
            }
        };
    }
}*/

public class EngineShader {
    private long   _fragShaderModTime;
    private string _vsFileName;
    private string _fsFileName;
    public  Shader Shader { get; set; }

    public        uint      Id   => Shader.Id;
    public unsafe Span<int> Locs => new((int*) Shader.Locs, (int) ShaderLocationIndex.LocBoneMatrices); 


    public EngineShader(Shader shader) {
        Shader = shader;
    }

    public EngineShader(string vsFileName, string fsFileName) {
        _vsFileName = vsFileName;
        _fsFileName = fsFileName;

        Shader             = LoadShader(vsFileName, fsFileName);
        _fragShaderModTime = GetFileModTime(fsFileName);

        // if (Context.ShaderHotReloading) EngineShaderWatcher.Shaders.Add(this);
    }

    public static implicit operator Shader(EngineShader shader) => shader.Shader;

    public void UpdateHotReload() {
        long currentFragShaderModTime = GetFileModTime(_fsFileName);
        if (currentFragShaderModTime != _fragShaderModTime) {
            _fragShaderModTime = currentFragShaderModTime;

            var updatedShader = LoadShader(_vsFileName, _fsFileName);
            if (updatedShader.Id != 0) {
                UnloadShader(Shader);
                Shader = updatedShader;
            }

            // Get shader locations for required uniforms
            // for (var i = 0; i < (int) ShaderLocationIndex.Count; i++) {
            //     Shader.Locs[i] = Raylib.GetShaderLocation(Shader, ((ShaderLocationIndex) i).ToString());
            // }
        }
    }

    /// <inheritdoc cref="Raylib.LoadShader" />
    public static EngineShader Load(string vsFileName, string fsFileName) {
        return new EngineShader(vsFileName, fsFileName);
    }

    /// <inheritdoc cref="Raylib.LoadShaderFromMemory" />
    public static EngineShader LoadFromMemory(string vsCode, string fsCode) {
        return new EngineShader(LoadShaderFromMemory(vsCode, fsCode));
    }

    /// <inheritdoc cref="Raylib.IsShaderReady" />
    public bool IsReady() {
        return IsShaderValid(Shader);
    }

    /// <inheritdoc cref="Raylib.UnloadShader" />
    public void Unload() {
        UnloadShader(Shader);
    }

    /// <inheritdoc cref="Raylib.GetShaderLocation" />
    public int GetLocation(string uniformName) {
        return GetShaderLocation(Shader, uniformName);
    }

    /// <inheritdoc cref="Raylib.GetShaderLocationAttrib" />
    public int GetLocationAttrib(string attribName) {
        return GetShaderLocationAttrib(Shader, attribName);
    }

    public unsafe void SetVector3(int locIndex, Vector3 value) {
        Raylib.SetShaderValue(Shader, locIndex, &value, (int)ShaderUniformDataType.Vec3);
    }

    public unsafe void SetVector4(int locIndex, Vector4 value) {
        Raylib.SetShaderValue(Shader, locIndex, &value, (int)ShaderUniformDataType.Vec4);
    }

    /// <inheritdoc cref="Raylib.SetShaderValue" />
    public unsafe void SetValue<T>(int locIndex, T value, ShaderUniformDataType uniformType) where T : unmanaged {
        Raylib.SetShaderValue(Shader, locIndex, &value, (int)uniformType);
    }

    /// <inheritdoc cref="Raylib.SetShaderValueV" />
    public unsafe void SetValueV<T>(int locIndex, Span<T> values, ShaderUniformDataType uniformType) where T : unmanaged {
        fixed (T* valuesPtr = values) {
            Raylib.SetShaderValueV(Shader, locIndex, valuesPtr, (int)uniformType, values.Length);
        }
    }

    public unsafe void SetValue<T>(string locName, T value) where T : unmanaged {
        var loc = GetLocation(locName);
        SetValue(loc, value);
    }
    public unsafe void SetValue<T>(int locIndex, T value) where T : unmanaged {
        switch (value) {
            case Vector2 v2:
                SetShaderValue(Shader, locIndex, &v2, (int)ShaderUniformDataType.Vec2);
                break;
            case Vector3 v3:
                SetShaderValue(Shader, locIndex, &v3, (int)ShaderUniformDataType.Vec3);
                break;
            case Vector4 v4:
                SetShaderValue(Shader, locIndex, &v4, (int)ShaderUniformDataType.Vec4);
                break;
            case Color c:
                SetShaderValue(Shader, locIndex, &c, (int)ShaderUniformDataType.Vec4);
                break;
            case float f:
                SetShaderValue(Shader, locIndex, &f, (int)ShaderUniformDataType.Float);
                break;
            case bool b:
                SetShaderValue(Shader, locIndex, &b, (int)ShaderUniformDataType.Int);
                break;
            case int i:
                SetShaderValue(Shader, locIndex, &i, (int)ShaderUniformDataType.Int);
                break;
            case Matrix4x4 m:
                SetShaderValueMatrix(Shader, locIndex, m);
                break;
            case Texture t:
                SetShaderValueTexture(Shader, locIndex, t);
                break;
            default:
                throw new NotSupportedException($"Type {value.GetType()} is not supported");
        }
    }

    /// <inheritdoc cref="Raylib.SetShaderValueMatrix" />
    public void SetValueMatrix(int locIndex, Matrix4x4 mat) {
        SetShaderValueMatrix(Shader, locIndex, mat);
    }

    /// <inheritdoc cref="Raylib.SetShaderValueTexture" />
    public void SetValueTexture(int locIndex, Texture texture) {
        SetShaderValueTexture(Shader, locIndex, texture);
    }

    public void Begin() => BeginShaderMode(Shader);
    public void End()   => EndShaderMode();
}