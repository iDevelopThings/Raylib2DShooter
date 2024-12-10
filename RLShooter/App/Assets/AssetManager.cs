using RLShooter.GameScene;
using RLShooter.GameScene.Rendering;
using RLShooter.Common.Utils.Collections;

namespace RLShooter.App.Assets;

public enum AssetHandleKind {
    None,
    Texture2D,
    Shader,
}

public class AssetHandleBase : IDisposable {
    protected object _asset;

    public string AssetPath { get; protected set; }

    public AssetHandleBase(string path) {
        AssetPath = path;
    }
    public virtual void Dispose() { }

    public virtual AssetHandleKind GetKind() => AssetHandleKind.None;

    public virtual bool IsValid() => _asset != null;
}

public class AssetHandle<T> : AssetHandleBase {
    public T Asset {
        get => (T) _asset;
        set => _asset = value;
    }

    public AssetHandle(string path, T asset) : base(path) {
        Asset = asset;
    }

    public static implicit operator T(AssetHandle<T> handle) => handle.Asset;
}

public class AssetDefinition {
    public virtual AssetHandleKind Kind => AssetHandleKind.None;
    public virtual Type            Type => null;

    public virtual List<string> Extensions { get; set; } = new();

    public static AssetDefinition_Texture2D Texture2D = new();
    public static AssetDefinition_Shader    Shader    = new();

    public static IEnumerable<AssetDefinition> All {
        get {
            yield return Texture2D;
            yield return Shader;
        }
    }

    public virtual AssetHandleBase Load(string           path)             => null;
    public virtual AssetHandleBase LoadFromObject(string path, object obj) => null;

    public T Load<T>(string path) where T : AssetHandleBase => (T) Load(path);
}

public class AssetDefinition_Texture2D : AssetDefinition {
    public override AssetHandleKind Kind       => AssetHandleKind.Texture2D;
    public override Type            Type       => typeof(Texture);
    public override List<string>    Extensions => new() {".png", ".jpg"};

    public override AssetHandleBase LoadFromObject(string path, object obj)
        => new AssetHandleTexture2D(path, (Texture) obj);

    public override AssetHandleBase Load(string path)
        => LoadFromObject(path, LoadTexture(path));
}

public class AssetHandleTexture2D : AssetHandle<Texture> {
    public AssetHandleTexture2D(string path, Texture asset) : base(path, asset) { }

    public static implicit operator Texture(AssetHandleTexture2D handle)
        => handle.Asset;

    public override AssetHandleKind GetKind() => AssetHandleKind.Texture2D;

    public override void Dispose() {
        UnloadTexture(Asset);
    }
    public override bool IsValid() {
        return IsTextureValid(Asset);
    }
}

public class AssetDefinition_Shader : AssetDefinition {
    public override AssetHandleKind Kind       => AssetHandleKind.Shader;
    public override Type            Type       => typeof(EngineShader);
    public override List<string>    Extensions => new() {".vs", ".fs"};


    public override AssetHandleBase LoadFromObject(string path, object obj)
        => new AssetHandleEngineShader(path, (EngineShader) obj);
}

public class AssetHandleEngineShader : AssetHandle<EngineShader> {
    public AssetHandleEngineShader(string path, EngineShader asset) : base(path, asset) { }

    public static implicit operator EngineShader(AssetHandleEngineShader handle)
        => handle.Asset;

    public override AssetHandleKind GetKind() => AssetHandleKind.Shader;

    public override void Dispose() {
        Asset.Unload();
    }
    public override bool IsValid() {
        return Asset.IsReady();
    }
}

[ContainerSingleton]
public class AssetManager : Singleton<AssetManager>, IDisposable {
    public static string SearchRootPrefix = "Resources";

    private Dictionary<string, AssetHandleBase> _handles = new();

    private List<KeyValuePair<string, string>> ShadersToLateLoad = new();

    public static string FixInputPath(string p) {
        p = p.Replace('\\', '/');

        if (p.StartsWith('/'))
            p = p[1..];

        // If we start with "Resources" we remove it
        if (p.StartsWith("Resources/"))
            p = p[10..];

        p = Path.Join(SearchRootPrefix, p);
        p = p.Replace('\\', '/');

        return p;
    }
    public static string ToShortAssetPath(
        string validAssetPath,
        string stripExtraPrefix = null,
        bool   stripExtensions  = false
    ) {
        var p = validAssetPath.Replace('\\', '/');

        if (p.StartsWith('/'))
            p = p[1..];

        // If we start with "Resources" we remove it
        if (p.StartsWith("Resources/"))
            p = p[10..];

        if (stripExtraPrefix != null) {
            if (stripExtraPrefix.StartsWith("Resources/"))
                stripExtraPrefix = stripExtraPrefix[10..];
            if (p.StartsWith(stripExtraPrefix))
                p = p[stripExtraPrefix.Length..];
        }

        if (p.StartsWith('/'))
            p = p[1..];

        if (stripExtensions) {
            var ext = Path.GetExtension(p);
            p = p.Replace(ext, "");
        }

        return p;
    }

    public void Initialize(Scene scene) {
        var loadedCounter = new Dictionary<string, int>();

        var allFiles = Directory.EnumerateFiles(SearchRootPrefix, "*", SearchOption.AllDirectories).ToList();

        var shaderFiles = allFiles
           .Where(f => AssetDefinition.Shader.Extensions.Contains(Path.GetExtension(f)))
           .ToList();

        allFiles.RemoveAll(f => shaderFiles.Contains(f));

        var shaderFileGroups = shaderFiles
           .GroupBy(Path.GetFileNameWithoutExtension)
           .ToList();
        shaderFileGroups.ForEach(g => {
            var vs = g.FirstOrDefault(f => f.EndsWith(".vs"));
            var fs = g.FirstOrDefault(f => f.EndsWith(".fs"));

            if (vs != null && fs != null) {
                ShadersToLateLoad.Add(new KeyValuePair<string, string>(FixInputPath(vs), FixInputPath(fs)));
                loadedCounter.Increment("Shader");
            }
        });

        foreach (var file in allFiles) {
            var ext      = Path.GetExtension(file);
            var filePath = FixInputPath(file);

            foreach (var def in AssetDefinition.All) {
                if (!def.Extensions.Contains(ext))
                    continue;
                var handle = def.Load(filePath);
                if (handle == null)
                    continue;

                _handles[ToShortAssetPath(filePath)] = handle;

                loadedCounter.Increment(def.Kind.ToString());
            }

            /*if (ext is ".png" or ".jpg") {
                LoadAsset<Texture2D>(filePath);
            }*/
        }


        Console.WriteLine("Loaded assets:");
        foreach (var (key, value) in loadedCounter) {
            Console.WriteLine($"\t{key}: {value}");
        }


        foreach (var (vs, fs) in ShadersToLateLoad) {
            var handle = new AssetHandleEngineShader(vs, EngineShader.Load(vs, fs));
            _handles[ToShortAssetPath(vs)] = handle;
        }
    }

    public static AssetHandle<T> LoadAsset<T>(string path) {
        var filePath = FixInputPath(path);

        if (Instance._handles.TryGetValue(filePath, out var handle)) {
            return handle as AssetHandle<T>;
        }

        var shortPath = ToShortAssetPath(filePath);

        foreach (var def in AssetDefinition.All) {
            if (def.Type != typeof(T))
                continue;

            Instance._handles[shortPath] = def.Load(filePath);

            return Instance._handles[shortPath] as AssetHandle<T>;
        }

        /*if (typeof(T) == typeof(Texture2D)) {
            var asset = (T) (object) Texture2D.Load(filePath);

            Instance._handles[shortPath] = new AssetHandleTexture2D(filePath, (Texture2D) (object) asset);

            return Instance._handles[shortPath] as AssetHandle<T>;
        }*/

        return null;
    }

    /// <summary>
    /// We take an input path(which would represent a directory) and load all assets in that directory
    /// </summary>
    /// <param name="path"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Dictionary<string, AssetHandle<T>> LoadAssets<T>(string path) {
        var relPath = FixInputPath(path);

        var allFiles = Directory.EnumerateFiles(relPath).ToList();

        var handles = new Dictionary<string, AssetHandle<T>>();

        foreach (var file in allFiles) {
            var filePath = FixInputPath(file);

            var handle     = GetOrLoadAsset<T>(filePath);
            var handlePath = ToShortAssetPath(filePath, relPath);

            if (handle != null) {
                handles.Add(ToShortAssetPath(filePath, relPath, true), handle);
            }
        }

        return handles;
    }

    public static AssetHandle<T> GetOrLoadAsset<T>(string path) {
        path = FixInputPath(path);
        var shortPath = ToShortAssetPath(path);

        if (Instance._handles.TryGetValue(shortPath, out var handle)) {
            return handle as AssetHandle<T>;
        }

        // bs catch case for shaders
        if (typeof(T) == typeof(EngineShader)) {
            // Strip the extension
            var p = Path.ChangeExtension(path, null);
            var pathsToTest = new List<string> {
                p + ".vs",
                p + ".fs",
            };

            foreach (var p2 in pathsToTest) {
                if (Instance._handles.TryGetValue(ToShortAssetPath(p2), out var handle2)) {
                    return handle2 as AssetHandle<T>;
                }
            }

            throw new Exception($"Shader not found: {path}");
        }

        return LoadAsset<T>(path);
    }

    public void Dispose() {
        foreach (var handle in _handles.Values) {
            handle.Dispose();
        }
        _handles.Clear();
    }
}