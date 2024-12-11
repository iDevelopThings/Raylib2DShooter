namespace RLShooter.Gameplay.Components;

public struct DestroyAfterTime {
    public float Time;
    public float Elapsed;
    public bool  Destroyed;

    public DestroyAfterTime(float time) {
        Time      = time;
        Elapsed   = 0;
        Destroyed = false;
    }
}

public struct QueuedForDestroy;