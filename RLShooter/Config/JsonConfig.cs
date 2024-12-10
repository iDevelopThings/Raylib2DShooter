using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace RLShooter.Config;

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;

public abstract class BaseJsonConfig { }

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class ConfigValueAttribute : Attribute;

[JsonObject(MemberSerialization = MemberSerialization.Fields, MissingMemberHandling = MissingMemberHandling.Ignore, ItemNullValueHandling = NullValueHandling.Ignore)]
public abstract class ConfigBase<T> : BaseJsonConfig, INotifyPropertyChanged where T : ConfigBase<T>, new() {
    private static          T      _instance;
    private static readonly string ConfigFilePath = $"{typeof(T).Name}.json";

    private static JsonSerializerSettings _jsonSettings = new() {
        Formatting            = Formatting.Indented,
        TypeNameHandling      = TypeNameHandling.Auto,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        MissingMemberHandling = MissingMemberHandling.Ignore,
        NullValueHandling     = NullValueHandling.Ignore,

    };

    public static T Instance {
        get {
            if (_instance == null) {
                _instance ??= Load();
            }

            return _instance;
        }
    }
    public static T GetInstance() => Instance;

    private static T Load() {
        if (File.Exists(ConfigFilePath)) {
            var json = File.ReadAllText(ConfigFilePath);
            var inst = JsonConvert.DeserializeObject<T>(json, _jsonSettings);

            return inst;
        }

        var newInstance = new T();
        newInstance.Save();
        return newInstance;
    }

    public static void SaveConfig() => Instance.Save();

    public void Save() {
        var json = JsonConvert.SerializeObject(this, Formatting.Indented, _jsonSettings);
        File.WriteAllText(ConfigFilePath, json);
    }

    protected void MarkDirty() {
        Save();
    }

    [NonSerialized]
    private PropertyChangedEventHandler _propertyChanged;
    public event PropertyChangedEventHandler PropertyChanged {
        add => _propertyChanged += value;
        remove => _propertyChanged -= value;
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
        MarkDirty();
        _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public bool SetField<FT>(ref FT field, FT value, [CallerMemberName] string propertyName = null) {
        if (EqualityComparer<FT>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}