using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RLShooter.GameScene;

public struct GameTime {
    public float Delta { get; set; }
    public float Time  { get; set; }
}

/// <summary>
/// Represents the time management system of the engine.
/// </summary>
public static class Time {
    private static          ulong frame;
    private static          long  last;
    private static          long  lastFixed;
    private static          float fixedTime;
    private static volatile float cumulativeFrameTime;
    private static volatile float gameTime;
    private static          float gameTimeScale = 20;
    private static volatile float delta;

    public static readonly object ProfileObject = new();
    
    /// <summary>
    /// Gets how many frames have passed since the start.
    /// </summary>
    public static ulong Frame => frame;

    /// <summary>
    /// Gets the time elapsed since the last frame in seconds.
    /// </summary>
    public static float Delta => delta;

    public static float DeltaTime => delta;

    /// <summary>
    /// Gets the cumulative frame time since the engine initialization in seconds.
    /// </summary>
    public static float CumulativeFrameTime => cumulativeFrameTime;

    /// <summary>
    /// Gets or sets the fixed update rate in frames per second (FPS).
    /// </summary>
    public static int FixedUpdateRate { get; set; } = 60;

    /// <summary>
    /// Gets the fixed time step size in seconds.
    /// </summary>
    public static float FixedDelta => 1f / FixedUpdateRate;

    /// <summary>
    /// Gets or sets the game time, is normalized in the 24h format.
    /// </summary>
    public static float GameTime {
        get => gameTime;
        set => gameTime = value % 24;
    }

    /// <summary>
    /// Gets or sets the game time scale, 1s real-time multiplied by scale.
    /// </summary>
    public static float GameTimeScale {
        get => gameTimeScale;
        set => gameTimeScale = value;
    }

    /// <summary>
    /// Occurs when a fixed update is triggered.
    /// </summary>
    public static event Action<float> FixedUpdate;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GameTime GetGameTime() => new() { Delta = Delta, Time = GameTime };
    
    /// <summary>
    /// Resets the time system.
    /// </summary>
    public static void ResetTime() {
        Interlocked.Exchange(ref last, 0);
        Interlocked.Exchange(ref lastFixed, 0);
        Interlocked.Exchange(ref fixedTime, 0);
        Interlocked.Exchange(ref cumulativeFrameTime, 0);
        Interlocked.Exchange(ref gameTime, 0);
    }

    /// <summary>
    /// Updates the Time system and calculates the delta time.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the delta time is 0 or less than 0.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void FrameUpdate() {
        Interlocked.Increment(ref frame);

        var now = Stopwatch.GetTimestamp();

        if (last == 0) {
            last  = now;
            delta = 1 / Stopwatch.Frequency;
            return;
        }

        var deltaTime = (double) (now - last) / Stopwatch.Frequency;
        last = now;

        delta = (float) deltaTime;
        // ReSharper disable once NonAtomicCompoundOperator
        cumulativeFrameTime += Delta;

        if (deltaTime == 0 || deltaTime < 0) {
            // To prevent problems set the delta to the minimum time possible.
            delta = 1 / Stopwatch.Frequency;
            return;
        }

        // ReSharper disable once NonAtomicCompoundOperator
        gameTime += (float) (deltaTime * gameTimeScale) / 60 / 60;
        // ReSharper disable once NonAtomicCompoundOperator
        gameTime %= 24;
    }

    /// <summary>
    /// Executes the fixed update tick based on a fixed time interval.
    /// </summary>
    /// <remarks>
    /// This method accumulates time between fixed update ticks and invokes the FixedUpdate event
    /// each time the accumulated time exceeds the fixed time interval.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when the delta time is 0 or less than 0.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FixedUpdateTick() {
        var now = Stopwatch.GetTimestamp();

        if (lastFixed == 0) {
            lastFixed = now;
            return;
        }

        var deltaTime = (double) (now - lastFixed) / Stopwatch.Frequency;
        lastFixed = now;

        fixedTime += (float) deltaTime;

        if (deltaTime == 0 || deltaTime < 0) {
            return;
        }

        while (fixedTime >= FixedDelta) {
            FixedUpdate?.Invoke(FixedDelta);
            fixedTime -= FixedDelta;
        }
    }

    /// <inheritdoc cref="RaylibApi.SetTargetFPS" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetTargetFPS(int fps) => Raylib.SetTargetFPS(fps);
    /// <inheritdoc cref="Raylib.GetFPS" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetFPS() => Raylib.GetFPS();

    /// <inheritdoc cref="Raylib.GetFrameTime" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetFrameTime() => Raylib.GetFrameTime();

    /// <inheritdoc cref="Raylib.GetTime" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double GetTotalTime() => Raylib.GetTime();

    /// <inheritdoc cref="Raylib.WaitTime" />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void WaitTime(double seconds) => Raylib.WaitTime(seconds);
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Since(double time) => GetTotalTime() - time;
}