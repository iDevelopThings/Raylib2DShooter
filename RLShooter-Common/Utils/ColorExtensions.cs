namespace RLShooter.Common.Utils;

public static class ColorExtensions {

    public static Vector4 Normalize(this Color @this) {
        var color = new Vector4 {
            X = (byte) (@this.R / 255f),
            Y = (byte) (@this.G / 255f),
            Z = (byte) (@this.B / 255f),
            W = (byte) (@this.A / 255f)
        };
        return color;
    }
    
    public static Color ToColor(this Vector4 @this) {
        var color = new Color {
            R = (byte) (@this.X * 255),
            G = (byte) (@this.Y * 255),
            B = (byte) (@this.Z * 255),
            A = (byte) (@this.W * 255)
        };
        return color;
    }
    public static Color ToRlColor(this Mathematics.Color @this) {
        var color = new Color {
            R = (byte) (@this.R * 255),
            G = (byte) (@this.G * 255),
            B = (byte) (@this.B * 255),
            A = (byte) (@this.A * 255)
        };
        return color;
    }
    

}