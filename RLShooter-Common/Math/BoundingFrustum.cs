﻿namespace RLShooter.Common.Mathematics;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a bounding frustum in 3D space defined by planes and corners.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct BoundingFrustum : IEquatable<BoundingFrustum>
{
    /// <summary>
    /// The number of corners in the bounding frustum.
    /// </summary>
    public const int CornerCount = 8;

    private readonly Plane[]   _planes;
    private readonly Vector3[] _corners;
    private          Vector3   _center;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingFrustum"/> struct.
    /// </summary>
    public BoundingFrustum()
    {
        _planes  = new Plane[6];
        _corners = new Vector3[CornerCount];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoundingFrustum"/> struct from a view-projection matrix.
    /// </summary>
    /// <param name="viewProjection">The view-projection matrix to create the frustum from.</param>
    public BoundingFrustum(Matrix4x4 viewProjection)
    {
        _planes =
        [
            Plane.Normalize(new Plane(-viewProjection.M13, -viewProjection.M23, -viewProjection.M33, -viewProjection.M43)),
            Plane.Normalize(new Plane(viewProjection.M13 - viewProjection.M14, viewProjection.M23 - viewProjection.M24, viewProjection.M33 - viewProjection.M34, viewProjection.M43 - viewProjection.M44)),
            Plane.Normalize(new Plane(-viewProjection.M14 - viewProjection.M11, -viewProjection.M24 - viewProjection.M21, -viewProjection.M34 - viewProjection.M31, -viewProjection.M44 - viewProjection.M41)),
            Plane.Normalize(new Plane(viewProjection.M11 - viewProjection.M14, viewProjection.M21 - viewProjection.M24, viewProjection.M31 - viewProjection.M34, viewProjection.M41 - viewProjection.M44)),
            Plane.Normalize(new Plane(viewProjection.M12 - viewProjection.M14, viewProjection.M22 - viewProjection.M24, viewProjection.M32 - viewProjection.M34, viewProjection.M42 - viewProjection.M44)),
            Plane.Normalize(new Plane(-viewProjection.M14 - viewProjection.M12, -viewProjection.M24 - viewProjection.M22, -viewProjection.M34 - viewProjection.M32, -viewProjection.M44 - viewProjection.M42)),
        ];

        _corners =
        [
            IntersectionPoint(_planes[0], _planes[2], _planes[4]),
            IntersectionPoint(_planes[0], _planes[3], _planes[4]),
            IntersectionPoint(_planes[0], _planes[3], _planes[5]),
            IntersectionPoint(_planes[0], _planes[2], _planes[5]),
            IntersectionPoint(_planes[1], _planes[2], _planes[4]),
            IntersectionPoint(_planes[1], _planes[3], _planes[4]),
            IntersectionPoint(_planes[1], _planes[3], _planes[5]),
            IntersectionPoint(_planes[1], _planes[2], _planes[5]),
        ];

        Vector3 center = new(0, 0, 0);
        for (int i = 0; i < _corners.Length; i++)
        {
            center += new Vector3(_corners[i].X, _corners[i].Y, _corners[i].Z);
        }
        _center /= _corners.Length;
    }

    /// <summary>
    /// Gets the corners of the bounding frustum.
    /// </summary>
    public IReadOnlyList<Vector3> Corners => _corners;

    /// <summary>
    /// Gets the planes of the bounding frustum.
    /// </summary>
    public IReadOnlyList<Plane> Planes => _planes;

    /// <summary>
    /// Gets the center of the bounding frustum.
    /// </summary>
    public readonly Vector3 Center => _center;

    /// <summary>
    /// Initializes the bounding frustum from a view-projection matrix.
    /// </summary>
    /// <param name="viewProjection">The view-projection matrix to create the frustum from.</param>
    public void Update(Matrix4x4 viewProjection)
    {
        _planes[0] = Plane.Normalize(new Plane(-viewProjection.M13, -viewProjection.M23, -viewProjection.M33, -viewProjection.M43));
        _planes[1] = Plane.Normalize(new Plane(viewProjection.M13 - viewProjection.M14, viewProjection.M23 - viewProjection.M24, viewProjection.M33 - viewProjection.M34, viewProjection.M43 - viewProjection.M44));
        _planes[2] = Plane.Normalize(new Plane(-viewProjection.M14 - viewProjection.M11, -viewProjection.M24 - viewProjection.M21, -viewProjection.M34 - viewProjection.M31, -viewProjection.M44 - viewProjection.M41));
        _planes[3] = Plane.Normalize(new Plane(viewProjection.M11 - viewProjection.M14, viewProjection.M21 - viewProjection.M24, viewProjection.M31 - viewProjection.M34, viewProjection.M41 - viewProjection.M44));
        _planes[4] = Plane.Normalize(new Plane(viewProjection.M12 - viewProjection.M14, viewProjection.M22 - viewProjection.M24, viewProjection.M32 - viewProjection.M34, viewProjection.M42 - viewProjection.M44));
        _planes[5] = Plane.Normalize(new Plane(-viewProjection.M14 - viewProjection.M12, -viewProjection.M24 - viewProjection.M22, -viewProjection.M34 - viewProjection.M32, -viewProjection.M44 - viewProjection.M42));

        _corners[0] = IntersectionPoint(_planes[0], _planes[2], _planes[4]);
        _corners[1] = IntersectionPoint(_planes[0], _planes[3], _planes[4]);
        _corners[2] = IntersectionPoint(_planes[0], _planes[3], _planes[5]);
        _corners[3] = IntersectionPoint(_planes[0], _planes[2], _planes[5]);
        _corners[4] = IntersectionPoint(_planes[1], _planes[2], _planes[4]);
        _corners[5] = IntersectionPoint(_planes[1], _planes[3], _planes[4]);
        _corners[6] = IntersectionPoint(_planes[1], _planes[3], _planes[5]);
        _corners[7] = IntersectionPoint(_planes[1], _planes[2], _planes[5]);

        Vector3 center = new(0, 0, 0);
        for (int i = 0; i < _corners.Length; i++)
        {
            center += new Vector3(_corners[i].X, _corners[i].Y, _corners[i].Z);
        }
        _center /= _corners.Length;
    }

    /// <summary>
    /// Checks whether the current <see cref="BoundingFrustum"/> intersects with a specified <see cref="BoundingBox"/>.
    /// </summary>
    /// <param name="box">The <see cref="BoundingBox"/> to check for intersection with the current <see cref="BoundingFrustum"/>.</param>
    /// <returns>True if intersects, false otherwise.</returns>
    public bool Intersects(BoundingBox box)
    {
        for (int i = 0; i < _planes.Length; i++)
        {
            Plane                 plane        = _planes[i];
            PlaneIntersectionType intersection = box.Intersects(in plane);

            if (intersection == PlaneIntersectionType.Front)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks whether the current <see cref="BoundingFrustum"/> intersects with a specified <see cref="BoundingSphere"/>.
    /// </summary>
    /// <param name="sphere">The <see cref="BoundingSphere"/> to check for intersection with the current <see cref="BoundingFrustum"/>.</param>
    /// <returns>True if intersects, false otherwise.</returns>
    public bool Intersects(BoundingSphere sphere)
    {
        for (int i = 0; i < _planes.Length; i++)
        {
            Plane                 plane        = _planes[i];
            PlaneIntersectionType intersection = sphere.Intersects(in plane);

            if (intersection == PlaneIntersectionType.Front)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Retrieves the eight corners of the bounding frustum.
    /// </summary>
    /// <returns>An array of points representing the eight corners of the bounding frustum.</returns>
    public void GetCorners(Vector3[] corners)
    {
#if NET8_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(corners);
#else
            if (corners is null)
            {
                throw new ArgumentNullException(nameof(corners));
            }
#endif
        if (corners.Length < CornerCount)
        {
            throw new ArgumentOutOfRangeException(nameof(corners), $"GetCorners need at least {CornerCount} elements to copy corners.");
        }

        _corners.CopyTo(corners, 0);
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        return obj is BoundingFrustum value && Equals(value);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(BoundingFrustum other)
    {
        for (int i = 0; i < _planes.Length; i++)
        {
            if (!_planes[i].Equals(other._planes[i]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Compares two <see cref="BoundingFrustum"/> objects for equality.
    /// </summary>
    /// <param name="left">The <see cref="BoundingFrustum"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="BoundingFrustum"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is equal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BoundingFrustum left, BoundingFrustum right) => left.Equals(right);

    /// <summary>
    /// Compares two <see cref="BoundingFrustum"/> objects for inequality.
    /// </summary>
    /// <param name="left">The <see cref="BoundingFrustum"/> on the left hand of the operand.</param>
    /// <param name="right">The <see cref="BoundingFrustum"/> on the right hand of the operand.</param>
    /// <returns>
    /// True if the current left is unequal to the <paramref name="right"/> parameter; otherwise, false.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BoundingFrustum left, BoundingFrustum right) => !left.Equals(right);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(_corners[0], _corners[1], _corners[2], _corners[3], _corners[4], _corners[5], _corners[6], _corners[7]);

    /// <inheritdoc />
    public override string ToString() => $"{nameof(BoundingFrustum)}";

    private static Vector3 IntersectionPoint(Plane a, Plane b, Plane c)
    {
        var cross = Vector3.Cross(b.Normal, c.Normal);

        float f = Vector3.Dot(a.Normal, cross);
        f *= -1.0f;

        var v1 = Vector3.Multiply(cross, a.D);

        cross = Vector3.Cross(c.Normal, a.Normal);
        var v2 = Vector3.Multiply(cross, b.D);

        cross = Vector3.Cross(a.Normal, b.Normal);
        var v3 = Vector3.Multiply(cross, c.D);

        Vector3 result;
        result.X = (v1.X + v2.X + v3.X) / f;
        result.Y = (v1.Y + v2.Y + v3.Y) / f;
        result.Z = (v1.Z + v2.Z + v3.Z) / f;

        return result;
    }
}