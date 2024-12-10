namespace RLShooter.Gameplay.Components;

public struct Named(string name) {
    public string Name { get; set; } = name;

    public static implicit operator Named(string name)  => new(name);
    public static implicit operator string(Named named) => named.Name;
    public override                 string ToString()   => Name;
}