﻿#if !MINIMAL
namespace RLShooter.Common.Mathematics
{
    using System.Numerics;

    /// <summary>
    /// A helper class for handling perspective shadow mapping (PSM) related operations.
    /// </summary>
    public static class PSMHelper
    {
        /// <summary>
        /// Gets a projection matrix for shadow mapping with the specified field of view and far clipping plane distance.
        /// </summary>
        /// <param name="fov">The field of view angle in radians.</param>
        /// <param name="far">The far clipping plane distance.</param>
        /// <returns>The projection matrix for shadow mapping.</returns>
        public static Matrix4x4 GetProjectionMatrix(float fov, float far)
        {
            return MathUtil.PerspectiveFovLH(fov, 1, 0.001f, far);
        }

        /// <summary>
        /// Gets a light space matrix for shadow mapping with custom far plane and frustum.
        /// </summary>
        /// <param name="light">The light source's <see cref="Transform"/>.</param>
        /// <param name="fov">The field of view angle in radians.</param>
        /// <param name="far">The far clipping plane distance.</param>
        /// <param name="frustum">The bounding frustum that determines the shadow mapping region.</param>
        /// <returns>The light space matrix for shadow mapping.</returns>
        public static unsafe Matrix4x4 GetLightSpaceMatrix(Transform light, float fov, float far, BoundingFrustum frustum)
        {
            Vector3 pos = light.GlobalPosition;
            Matrix4x4 proj = GetProjectionMatrix(fov, far);
            Matrix4x4 viewproj = MathUtil.LookAtLH(pos, pos + light.Forward, light.Up) * proj;
            frustum.Update(viewproj);
            return Matrix4x4.Transpose(viewproj);
        }

        /// <summary>
        /// Gets a light space matrix for shadow mapping with custom far plane and frustum.
        /// </summary>
        /// <param name="light">The light source's <see cref="Transform"/>.</param>
        /// <param name="fov">The field of view angle in radians.</param>
        /// <param name="far">The far clipping plane distance.</param>
        /// <param name="frustum">The bounding frustum that determines the shadow mapping region.</param>
        /// <param name="view">The light view space matrix for shadow mapping.</param>
        /// <param name="viewProjection">The light space matrix for shadow mapping.</param>
        /// <returns></returns>
        public static unsafe void GetLightSpaceMatrix(Transform light, float fov, float far, BoundingFrustum frustum, Matrix4x4* view, Matrix4x4* viewProjection)
        {
            Vector3 pos = light.GlobalPosition;
            Matrix4x4 viewMat = MathUtil.LookAtLH(pos, pos + light.Forward, light.Up);
            Matrix4x4 proj = GetProjectionMatrix(fov, far);
            Matrix4x4 viewProjectionMat = viewMat * proj;
            frustum.Update(viewProjectionMat);
            *view = Matrix4x4.Transpose(viewMat);
            *viewProjection = Matrix4x4.Transpose(viewProjectionMat);
        }
    }
}
#endif