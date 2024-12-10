using System.Reflection;
using MethodDecorator.Fody.Interfaces;

namespace RLShooter.App.Profiling;

// disable warning CS9113
#pragma warning disable 9113
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
public class ProfileAttribute(string optionalName = null) : Attribute
#if PROFILE
                              , IMethodDecorator
#endif
{
#if PROFILE
    private string name = optionalName;

    public void Init(object instance, MethodBase method, object[] args) {
        if (name == null)
            name = $"{method.DeclaringType!.Name}.{method.Name}";
    }

    public void OnEntry() => Profiler.Instance.Start(name);

    public void OnExit() => Profiler.Instance.Stop(name);

    public void OnException(Exception exception) { }

#endif
}

#pragma warning restore 9113