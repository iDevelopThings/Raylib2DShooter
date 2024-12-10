namespace RLShooter.App.Editor.Windows.Inspectors;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class InspectorAttribute : Attribute {
    public Type Type { get; }

    public InspectorAttribute(Type type) {
        Type = type;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class HideInInspectorAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class DisabledInInspectorAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorOrder(int order) : Attribute {
    public int Order { get; set; } = order;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorCategory(string category) : Attribute {
    public string Category { get; set; } = category;
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorShowNameOnly : Attribute;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorMinMax : Attribute {
    public float Min { get; set; }
    public float Max { get; set; }

    public InspectorMinMax(float min, float max) {
        Min = min;
        Max = max;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class InspectorAdvanced : Attribute;