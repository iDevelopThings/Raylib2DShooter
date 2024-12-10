using System.Numerics;
using System.Runtime.CompilerServices;

namespace RLShooter.Common.Utils;

public static class QuaternionExtensions {

    /// <summary>
    /// The factor to convert degrees to radians.
    /// </summary>
    public const double DegToRadFactor = MathF.PI / 180;

    /// <summary>
    /// The factor to convert radians to degrees.
    /// </summary>
    public const double RadToDefFactor = 180 / MathF.PI;

    /// <summary>
    /// The mathematical constant PI.
    /// </summary>
    public const float PI = MathF.PI;

    /// <summary>
    /// Two times the mathematical constant PI.
    /// </summary>
    public const float PI2 = 2 * MathF.PI;

    /// <summary>
    /// Half of the mathematical constant PI.
    /// </summary>
    public const float PIDIV2 = MathF.PI / 2;

    /// <summary>
    /// The square root of 2.
    /// </summary>
    public const float SQRT2 = 1.41421356237309504880f;

    /// <summary>
    /// The square root of 3.
    /// </summary>
    public const float SQRT3 = 1.73205080756887729352f;

    /// <summary>
    /// The square root of 6.
    /// </summary>
    public const float SQRT6 = 2.44948974278317809820f;

    public static (Vector3 axis, float angle) ToAxisAngle(this Quaternion q) {
        if (q.LengthSquared() == 0)
            throw new ArgumentException("Quaternion must not be zero.");

        // Normalize the quaternion to avoid scaling issues
        q = Quaternion.Normalize(q);

        // Extract the angle
        float angle = 2.0f * (float) Math.Acos(q.W);

        // Calculate the denominator for the axis calculation
        float sinHalfAngle = (float) Math.Sqrt(1.0f - q.W * q.W);

        // Avoid division by zero
        Vector3 axis;
        if (sinHalfAngle > 0.0001f) {
            axis = new Vector3(q.X, q.Y, q.Z) / sinHalfAngle;
        } else {
            // Default axis if the angle is zero
            axis = new Vector3(1, 0, 0);
        }

        return (axis, angle);
    }

    public static float ToDegrees(this Quaternion @this) {
        var (axis, rotationAngle) = @this.ToAxisAngle();
        return float.RadiansToDegrees(rotationAngle);
    }
    /// <summary>
    /// Calculates yaw, pitch, and roll angles from a <see cref="Quaternion"/> and stores them in the provided out parameters.
    /// </summary>
    /// <param name="r">The input <see cref="Quaternion"/>.</param>
    /// <param name="yaw">The calculated yaw angle (in radians).</param>
    /// <param name="pitch">The calculated pitch angle (in radians).</param>
    /// <param name="roll">The calculated roll angle (in radians).</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetYawPitchRoll(this Quaternion r, out float yaw, out float pitch, out float roll) {
        yaw   = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
        pitch = MathF.Asin(Math.Clamp(2.0f * (r.X * r.W - r.Y * r.Z), -1, 1));
        roll  = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
    }

    /// <summary>
    /// Calculates yaw, pitch, and roll angles from a <see cref="Quaternion"/> and returns them as a <see cref="Vector3"/>.
    /// </summary>
    /// <param name="r">The input <see cref="Quaternion"/>.</param>
    /// <returns>A <see cref="Vector3"/> containing yaw, pitch, and roll angles (in radians).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToYawPitchRoll(this Quaternion r) {
        var yaw   = MathF.Atan2(2.0f * (r.Y * r.W + r.X * r.Z), 1.0f - 2.0f * (r.X * r.X + r.Y * r.Y));
        var pitch = MathF.Asin(Math.Clamp(2.0f * (r.X * r.W - r.Y * r.Z), -1, 1));
        var roll  = MathF.Atan2(2.0f * (r.X * r.Y + r.Z * r.W), 1.0f - 2.0f * (r.X * r.X + r.Z * r.Z));
        return new Vector3(yaw, pitch, roll);
    }

    /// <summary>
    /// Converts a <see cref="Vector3"/> from radians to degrees.
    /// </summary>
    /// <param name="v">The input vector in radians.</param>
    /// <returns>A <see cref="Vector3"/> with values converted to degrees.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToDeg(this Vector3 v) {
        return new Vector3((float) (v.X * RadToDefFactor), (float) (v.Y * RadToDefFactor), (float) (v.Z * RadToDefFactor));
    }

    /// <summary>
    /// Converts a <see cref="Vector3"/> from degrees to radians.
    /// </summary>
    /// <param name="v">The input vector in degrees.</param>
    /// <returns>A <see cref="Vector3"/> with values converted to radians.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 ToRad(this Vector3 v) {
        return new Vector3((float) (v.X * DegToRadFactor), (float) (v.Y * DegToRadFactor), (float) (v.Z * DegToRadFactor));
    }

    /// <summary>
    /// Converts an angle from radians to degrees.
    /// </summary>
    /// <param name="v">The input angle in radians.</param>
    /// <returns>The angle value converted to degrees.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToDeg(this float v) {
        return v == 0 ? 0.0f : (float) (v * RadToDefFactor);
    }

    /// <summary>
    /// Converts an angle from degrees to radians.
    /// </summary>
    /// <param name="v">The input angle in degrees.</param>
    /// <returns>The angle value converted to radians.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ToRad(this float v) {
        return (float) (v * DegToRadFactor);
    }

    /// <summary>
    /// Normalizes Euler angles to the [0, 360] degree range.
    /// </summary>
    /// <param name="angle">The input Euler angles.</param>
    /// <returns>The normalized Euler angles.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 NormalizeEulerAngleDegrees(this Vector3 angle) {
        var normalizedX = angle.X % 360;
        var normalizedY = angle.Y % 360;
        var normalizedZ = angle.Z % 360;
        if (normalizedX < 0) {
            normalizedX += 360;
        }

        if (normalizedY < 0) {
            normalizedY += 360;
        }

        if (normalizedZ < 0) {
            normalizedZ += 360;
        }

        return new(normalizedX, normalizedY, normalizedZ);
    }

    /// <summary>
    /// Converts a <see cref="Vector3"/> representing yaw, pitch, and roll angles to a quaternion.
    /// </summary>
    /// <param name="vector">The input <see cref="Vector3"/> with yaw, pitch, and roll angles.</param>
    /// <returns>The corresponding <see cref="Quaternion"/> representing the rotation.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Quaternion ToQuaternion(this Vector3 vector) {
        return Quaternion.CreateFromYawPitchRoll(vector.X, vector.Y, vector.Z);
    }
}