using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using fennecs;

#if !PROFILE
using System.Diagnostics.Metrics;
#endif

namespace RLShooter.Gameplay.Systems;

/// <summary>
///     An interface providing several methods for a system. 
/// </summary>
public interface ISystem : IDisposable {
    /// <summary>
    ///     Initializes a system, before its first ever run.
    /// </summary>
    void Initialize();

    /// <summary>
    ///     Runs before <see cref="Update"/>.
    /// </summary>
    /// <param name="t">An instance passed to it.</param>
    void BeforeUpdate(in float t);

    /// <summary>
    ///     Updates the system.
    /// </summary>
    /// <param name="t">An instance passed to it.</param>
    void Update(in float t);

    /// <summary>
    ///     Runs after <see cref="Update"/>.
    /// </summary>
    /// <param name="t">An instance passed to it.</param>
    void AfterUpdate(in float t);
}

public abstract class BaseSystem : ISystem {

    /// <summary>
    ///     Creates an instance. 
    /// </summary>
    /// <param name="world">The <see cref="World"/>.</param>
    protected BaseSystem(World world) {
        World = world;
    }

    /// <summary>
    ///     The world instance. 
    /// </summary>
    public World World { get; private set; }

    /// <inheritdoc />
    public virtual void Initialize() { }

    /// <inheritdoc />
    public virtual void BeforeUpdate(in float t) { }

    /// <inheritdoc />
    public virtual void Update(in float t) { }

    /// <inheritdoc />
    public virtual void AfterUpdate(in float t) { }

    /// <inheritdoc />
    public virtual void Dispose() { }
}

public sealed class Group<T> : ISystem {
#if !PROFILE
    private readonly Meter _meter;
    private readonly Stopwatch _timer = new();
#endif

    /// <summary>
    /// A unique name to identify this group
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// All <see cref="SystemEntry"/>'s in this group. 
    /// </summary>
    private readonly List<SystemEntry> _systems = new();

    /// <summary>
    ///     Creates an instance with an array of <see cref="ISystem"/>'s that will belong to this group.
    /// </summary>
    /// <param name="name">A unique name to identify this group</param>
    /// <param name="systems">An <see cref="ISystem"/> array.</param>
    public Group(string name, params ISystem[] systems)
        : this(name, (IEnumerable<ISystem>) systems) { }

    /// <summary>
    ///     Creates an instance with an <see cref="IEnumerable{T}"/> of <see cref="ISystem"/>'s that will belong to this group.
    /// </summary>
    /// <param name="name">A unique name to identify this group</param>
    /// <param name="systems">An <see cref="IEnumerable{T}"/> of <see cref="ISystem"/>.</param>
    public Group(string name, IEnumerable<ISystem> systems) {
        Name = name;

#if !PROFILE
        _meter = new Meter(name);
#endif

#if NET5_0_OR_GREATER
        // If possible expand the list before adding all the systems
        if (systems.TryGetNonEnumeratedCount(out var count))
            _systems.Capacity = count;
#endif

        foreach (var system in systems)
            Add(system);
    }

    /// <summary>
    ///     Adds several new <see cref="ISystem"/>'s to this group.
    /// </summary>
    /// <param name="systems">An <see cref="ISystem"/> array.</param>
    /// <returns>The same <see cref="Group{T}"/>.</returns>
    public Group<T> Add(params ISystem[] systems) {
        _systems.Capacity = Math.Max(_systems.Capacity, _systems.Count + systems.Length);

        foreach (var system in systems)
            Add(system);

        return this;
    }

    /// <summary>
    ///     Adds an single <see cref="ISystem"/> to this group by its generic.
    ///     Automatically initializes it properly. Must be contructorless.
    /// </summary>
    /// <typeparam name="G">Its generic type.</typeparam>
    /// <returns>The same <see cref="Group{T}"/>.</returns>
    public Group<T> Add<G>() where G : ISystem, new() {
        return Add(new G());
    }

    /// <summary>
    ///     Adds an single <see cref="ISystem"/> to this group.
    /// </summary>
    /// <param name="system"></param>
    /// <returns></returns>
    public Group<T> Add(ISystem system) {
        _systems.Add(new SystemEntry(system
#if !PROFILE
            , _meter
#endif
                     ));

        return this;
    }

    /// <summary>
    ///     Return the first <see cref="G"/> which was found in the hierachy.
    /// </summary>
    /// <typeparam name="G">The Type.</typeparam>
    /// <returns></returns>
    public G Get<G>() where G : ISystem {
        foreach (var item in _systems) {
            if (item.System is G sys) {
                return sys;
            }

            if (item.System is not Group<T> grp) {
                continue;
            }

            return grp.Get<G>();
        }

        return default;
    }

    /// <summary>
    ///     Finds all <see cref="ISystem"/>s which can be cast into the given type.
    /// </summary>
    /// <typeparam name="G">The Type.</typeparam>
    /// <returns></returns>
    public IEnumerable<G> Find<G>() where G : ISystem {
        foreach (var item in _systems) {
            if (item.System is G sys) {
                yield return sys;
            }

            if (item.System is not Group<T> grp) {
                continue;
            }

            foreach (var nested in grp.Find<G>()) {
                yield return nested;
            }
        }
    }

    /// <summary>
    ///     Initializes all <see cref="ISystem"/>'s in this <see cref="Group{T}"/>.
    /// </summary>
    public void Initialize() {
        for (var index = 0; index < _systems.Count; index++) {
            var entry = _systems[index];
            entry.System.Initialize();
        }
    }

    /// <summary>
    ///     Runs <see cref="ISystem.BeforeUpdate"/> on each <see cref="ISystem"/> of this <see cref="Group{T}"/>..
    /// </summary>
    /// <param name="t">An instance passed to each <see cref="ISystem.Initialize"/> method.</param>
    public void BeforeUpdate(in float t) {
        for (var index = 0; index < _systems.Count; index++) {
            var entry = _systems[index];

#if !PROFILE
            _timer.Restart();
            {
#endif

            entry.System.BeforeUpdate(in t);

#if !PROFILE
            }
            _timer.Stop();
            entry.BeforeUpdate.Record(_timer.Elapsed.TotalMilliseconds);
#endif
        }
    }

    /// <summary>
    ///     Runs <see cref="ISystem.Update"/> on each <see cref="ISystem"/> of this <see cref="Group{T}"/>..
    /// </summary>
    /// <param name="t">An instance passed to each <see cref="ISystem.Initialize"/> method.</param>
    public void Update(in float t) {
        for (var index = 0; index < _systems.Count; index++) {
            var entry = _systems[index];

#if !PROFILE
            _timer.Restart();
            {
#endif

            entry.System.Update(in t);

#if !PROFILE
            }
            _timer.Stop();
            entry.Update.Record(_timer.Elapsed.TotalMilliseconds);
#endif
        }
    }

    /// <summary>
    ///     Runs <see cref="ISystem.AfterUpdate"/> on each <see cref="ISystem"/> of this <see cref="Group{T}"/>..
    /// </summary>
    /// <param name="t">An instance passed to each <see cref="ISystem.Initialize"/> method.</param>
    public void AfterUpdate(in float t) {
        for (var index = 0; index < _systems.Count; index++) {
            var entry = _systems[index];

#if !PROFILE
            _timer.Restart();
            {
#endif

            entry.System.AfterUpdate(in t);

#if !PROFILE
            }
            _timer.Stop();
            entry.AfterUpdate.Record(_timer.Elapsed.TotalMilliseconds);
#endif
        }
    }

    /// <summary>
    ///     Disposes this <see cref="Group{T}"/> and all <see cref="ISystem"/>'s within.
    /// </summary>
    public void Dispose() {
        foreach (var system in _systems) {
            system.Dispose();
        }
    }

    /// <summary>
    ///     Converts this <see cref="Group{T}"/> to a human readable string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        // List all system names
        var stringBuilder = new StringBuilder();
        foreach (var systemEntry in _systems) {
            stringBuilder.Append($"{systemEntry.System.GetType().Name},");
        }

        // Cut last `,`
        if (_systems.Count > 0) {
            stringBuilder.Length--;
        }

        return $"Group = {{ {nameof(Name)} = {Name}, Systems = {{ {stringBuilder} }} }} ";
    }

    /// <summary>
    ///     The struct <see cref="SystemEntry"/> represents the given <see cref="ISystem"/> in the <see cref="Group{T}"/> with all its performance statistics.
    /// </summary>
    private readonly struct SystemEntry : IDisposable {
        public readonly ISystem System;

#if !PROFILE
        public readonly Histogram<double> BeforeUpdate;
        public readonly Histogram<double> Update;
        public readonly Histogram<double> AfterUpdate;
#endif

        public void Dispose() {
            System.Dispose();
        }

        public SystemEntry(
            ISystem system
#if !PROFILE
                , Meter meter
#endif
        ) {
            var name = system.GetType().Name;
            System = system;

#if !PROFILE
            BeforeUpdate = meter.CreateHistogram<double>($"{name}.BeforeUpdate", unit: "millisecond");
            Update = meter.CreateHistogram<double>($"{name}.Update", unit: "millisecond");
            AfterUpdate = meter.CreateHistogram<double>($"{name}.AfterUpdate", unit: "millisecond");
#endif
        }
    }
}