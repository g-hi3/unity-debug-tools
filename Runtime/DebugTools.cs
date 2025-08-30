using UnityEngine;

namespace G_hi3.Debug
{
    /// <summary>
    /// Provides custom debug utility methods.
    /// </summary>
    public static class DebugTools
    {
        private const float DefaultSegmentLength = 0.4f;
        private const float DefaultSegmentSpacing = 0.2f;
        private const float DefaultTimeScale = 0f;
        private const bool DefaultDepthTest = true;
        private const bool DefaultLoop = false;
        private const uint DefaultSegmentCount = 4u;
        private static readonly Quaternion RightRotation = Quaternion.FromToRotation(Vector3.up, Vector3.right);
        private static readonly Quaternion ForwardRotation = Quaternion.FromToRotation(Vector3.up, Vector3.forward);
        
        /// <summary>
        /// <para>
        /// Draws a segmented line between the corners of a rectangle in world space.
        /// By default, the segmented rectangle is static,
        /// but it can be animated using the <paramref name="timeScale"/> parameter.
        /// </para>
        /// <para>
        /// A timescale of 1 means that the line segments move 1 world unit per second.
        /// The default timescale of 0 means that the line segments are not animated.
        /// There is no performance benefit of not animating the line segments
        /// because the animation does not add any overhead. 
        /// </para>
        /// </summary>
        /// <param name="position">the center of the rectangle in world space</param>
        /// <param name="rotation">the rotation of the rectangle</param>
        /// <param name="scale">the size of the rectangle (width, height)</param>
        /// <param name="segmentLength">the length of unclipped segments in the line</param>
        /// <param name="segmentSpacing">the length of spacing between line segments</param>
        /// <param name="segmentColor">color of the line segments; default is white</param>
        /// <param name="timeScale">determines the animation speed</param>
        /// <param name="depthTest">does not perform depth tests if <c>false</c></param>
        public static void DrawSegmentedRectangle(
            Vector3 position,
            Quaternion rotation,
            Vector2 scale,
            float segmentLength = DefaultSegmentLength,
            float segmentSpacing = DefaultSegmentSpacing,
            Color segmentColor = default,
            float timeScale = DefaultTimeScale,
            bool depthTest = DefaultDepthTest)
        {
#if DEBUG
            var halfScale = 0.5f * scale;
            var invertedHalfScale = new Vector3(halfScale.x, -halfScale.y, 0f);
            
            var corners = new[]
            {
                position + rotation * -invertedHalfScale,
                position + rotation * -halfScale,
                position + rotation * invertedHalfScale,
                position + rotation * halfScale
            };
            
            DrawSegmentedPath(corners, segmentLength, segmentSpacing, segmentColor, timeScale, depthTest, true);
#endif
        }

        /// <summary>
        /// <para>
        /// Draws a segmented line between two or more points in world space.
        /// By default, the segmented path is static,
        /// but it can be animated using the <paramref name="timeScale"/> parameter.
        /// </para>
        /// <para>
        /// A timescale of 1 means that the line segments move 1 world unit per second.
        /// The default timescale of 0 means that the line segments are not animated.
        /// There is no performance benefit of not animating the line segments
        /// because the animation does not add any overhead. 
        /// </para>
        /// </summary>
        /// <param name="positions">all positions in the path</param>
        /// <param name="segmentLength">the length of unclipped segments in the line</param>
        /// <param name="segmentSpacing">the length of spacing between line segments</param>
        /// <param name="segmentColor">color of the line segments; default is white</param>
        /// <param name="timeScale">determines the animation speed</param>
        /// <param name="depthTest">does not perform depth tests if <c>false</c></param>
        /// <param name="loop">if <c>true</c>, connects the last point with the first</param>
		public static void DrawSegmentedPath(
            Vector3[] positions,
            float segmentLength = DefaultSegmentLength,
            float segmentSpacing = DefaultSegmentSpacing,
            Color segmentColor = default,
            float timeScale = DefaultTimeScale,
            bool depthTest = DefaultDepthTest,
            bool loop = DefaultLoop)
		{
#if DEBUG
            if (positions.Length < 2)
                return;
            
            var overshoot = 0f;
            
            for (var i = 0; i < positions.Length - 1; i++)
            {
                overshoot = DrawSegmentedLineInternal(
                    positions[i],
                    positions[i+1],
                    segmentLength,
                    segmentSpacing,
                    segmentColor,
                    overshoot,
                    timeScale,
                    depthTest);
            }

            if (loop)
            {
                _ = DrawSegmentedLineInternal(
                    positions[^1],
                    positions[0],
                    segmentLength,
                    segmentSpacing,
                    segmentColor,
                    overshoot,
                    timeScale,
                    depthTest);
            }
#endif
        }
        
        /// <summary>
        /// <para>
        /// Draws a segmented line between two points in world space.
        /// By default, the segmented line is static,
        /// but it can be animated using the <paramref name="timeScale"/> parameter.
        /// </para>
        /// <para>
        /// A timescale of 1 means that the line segments move 1 world unit per second.
        /// The default timescale of 0 means that the line segments are not animated.
        /// There is no performance benefit of not animating the line segments
        /// because the animation does not add any overhead. 
        /// </para>
        /// </summary>
        /// <param name="start">the line's starting point</param>
        /// <param name="end">the line's ending point</param>
        /// <param name="segmentLength">the length of unclipped segments in the line</param>
        /// <param name="segmentSpacing">the length of spacing between line segments</param>
        /// <param name="segmentColor">color of the line segments; default is white</param>
        /// <param name="timeScale">determines the animation speed</param>
        /// <param name="depthTest">does not perform depth tests if <c>false</c></param>
        /// <remarks>
        /// <para>
        /// Line drawing is skipped if <paramref name="segmentLength"/> is less than or equal to 0.
        /// If <paramref name="segmentSpacing"/> is less or equal to 0,
        /// a single line-to operation will be performed from <paramref cref="start"/> to <paramref name="end"/>.
        /// </para>
        /// <para>
        /// The line drawing is skipped completely
        /// if <paramref name="segmentColor"/>'s <see cref="Color.a"/> value is 0.
        /// However, the color <c>(0, 0, 0, 0)</c> will be replaced by white as the default color.
        /// </para>
        /// </remarks>
        public static void DrawSegmentedLine(
            Vector3 start,
            Vector3 end,
            float segmentLength = DefaultSegmentLength,
            float segmentSpacing = DefaultSegmentSpacing,
            Color segmentColor = default,
            float timeScale = DefaultTimeScale,
            bool depthTest = DefaultDepthTest)
        {
#if DEBUG
            _ = DrawSegmentedLineInternal(
                start,
                end,
                segmentLength,
                segmentSpacing,
                segmentColor,
                0f,
                timeScale,
                depthTest);
#endif
        }

#if DEBUG
        private static float DrawSegmentedLineInternal(
            Vector3 start,
            Vector3 end,
            float segmentLength,
            float segmentSpacing,
            Color segmentColor,
            float overshoot,
            float timeScale,
            bool depthTest)
        {
            if (segmentColor == default)
            {
                segmentColor = Color.white;
            }
            else if (segmentColor.a <= 0f)
            {
                return 0f;
            }

            if (segmentLength <= 0f)
                return 0f;
            
            var delta = end - start;
            var deltaLength = delta.magnitude;
            
            if (segmentSpacing <= 0f)
            {
                DrawSegment(start, end, segmentColor, depthTest);
                return segmentSpacing - deltaLength;
            }

            var direction = delta.normalized;
            var unitLength = segmentLength + segmentSpacing;

            var offset = overshoot > 0f
                ? overshoot
                : Time.time * timeScale % unitLength;
            
            var distanceTravelled = offset;

            if (distanceTravelled < 0f)
            {
                // TODO: Is $n$ ever greater than 1 here?
                var n = (int)(-distanceTravelled % unitLength);

                if (n > 1)
                    n = 1;

                distanceTravelled += n * unitLength;

                if (segmentSpacing > -distanceTravelled)
                {
                    // TODO: If `segmentLength` is large, the first line sometimes doesn't show this segment.
                    var segmentEndDistance = Mathf.Clamp(distanceTravelled + segmentLength, 0f, deltaLength);
                    var segmentEnd = start + segmentEndDistance * direction;
                    var segmentStartDistance = Mathf.Clamp(distanceTravelled, 0f, deltaLength);
                    var segmentStart = start + segmentStartDistance * direction;
                    DrawSegment(segmentStart, segmentEnd, segmentColor, depthTest);
                }

                distanceTravelled += unitLength;
            }
            else if (distanceTravelled > 0f)
            {
                var segmentEndDistance = distanceTravelled - segmentSpacing;
                var offsetSegmentLength = Mathf.Min(segmentEndDistance, deltaLength);
                
                if (offsetSegmentLength > 0)
                {
                    var segmentEnd = start + offsetSegmentLength * direction;
                    var segmentStartDistance = Mathf.Max(offsetSegmentLength - segmentLength, 0f);
                    var segmentStart = start + segmentStartDistance * direction;
                    DrawSegment(segmentStart, segmentEnd, segmentColor, depthTest);
                }
            }

            while (distanceTravelled < deltaLength)
            {
                var clampedStartDistance = Mathf.Clamp(distanceTravelled, 0f, deltaLength);
                var segmentStart = start + clampedStartDistance * direction;
                var maxSegmentLength = Mathf.Clamp(deltaLength - distanceTravelled, 0f, segmentLength);
                var segment = Vector3.ClampMagnitude(segmentLength * direction, maxSegmentLength);
                var segmentEnd = segmentStart + segment;
                DrawSegment(segmentStart, segmentEnd, segmentColor, depthTest);
                distanceTravelled += unitLength;
            }

            return distanceTravelled - deltaLength;
        }

        private static void DrawSegment(Vector3 start, Vector3 end, Color segmentColor, bool depthTest)
        {
            UnityEngine.Debug.DrawLine(start, end, segmentColor, 0f, depthTest);
        }
#endif
        
        /// <summary>
        /// Draws a raycast with collision, if any.
        /// </summary>
        /// <param name="ray">ray to cast</param>
        /// <param name="hit">the raycast hit, if any</param>
        /// <param name="maxDistance">max distance to draw</param>
        /// <param name="collisionSphereRadius">the radius of the drawn collision sphere</param>
        /// <param name="segmentCount">number of segments per circle on the collision sphere</param>
        /// <param name="hitColor">color of the ray between the origin and the hit, if any</param>
        /// <param name="noHitColor">color of the ray after the hit, if there's remaining length or no hit</param>
        /// <param name="collisionColor">color of the collision sphere</param>
        /// <param name="duration">how long the line and sphere will be visible for in seconds</param>
        /// <param name="depthTest">does not perform depth tests if <c>false</c></param>
        public static void DrawRaycast(
            Ray ray,
            RaycastHit? hit = null,
            float maxDistance = float.PositiveInfinity,
            float collisionSphereRadius = 0.1f,
            uint segmentCount = DefaultSegmentCount,
            Color hitColor = default,
            Color noHitColor = default,
            Color collisionColor = default,
            float duration = 0f,
            bool depthTest = true)
        {
#if DEBUG
            if (maxDistance < 0f)
                return;

            if (noHitColor == default)
                noHitColor = Color.red;

            if (!hit.HasValue)
            {
                UnityEngine.Debug.DrawRay(ray.origin, ray.direction * maxDistance, noHitColor, duration, depthTest);
                return;
            }

            if (hitColor == default)
                hitColor = Color.green;

            if (collisionColor == default)
                collisionColor = Color.blue;

            var hitValue = hit.Value;
            UnityEngine.Debug.DrawLine(ray.origin, hitValue.point, noHitColor, duration, depthTest);
            var sphereScale = collisionSphereRadius * Vector3.one;
            var sphereRotation = Quaternion.Euler(hitValue.normal);
            DrawWireframeSphere(hitValue.point, sphereRotation, sphereScale, segmentCount, collisionColor, duration, depthTest);
            var newRay = new Ray(hitValue.point, ray.direction);
            DrawRay(newRay, maxDistance - hitValue.distance, hitColor, duration, depthTest);
#endif
        }

#if DEBUG
        private static void DrawRay(Ray ray, float maxDistance, Color color, float duration, bool depthTest)
        {
            if (float.IsInfinity(maxDistance))
            {
                UnityEngine.Debug.DrawRay(ray.origin, ray.direction, color, duration, depthTest);
            }
            else
            {
                var end = ray.origin + maxDistance * ray.direction;
                UnityEngine.Debug.DrawLine(ray.origin, end, color, duration, depthTest);
            }
        }
#endif

        /// <summary>
        /// Draws a wireframe sphere
        /// </summary>
        /// <param name="position">center of the sphere</param>
        /// <param name="rotation">rotation of the sphere around the <paramref name="position"/></param>
        /// <param name="scale">scale of the sphere, independent of <paramref name="rotation"/></param>
        /// <param name="segmentCount">number of segments per circle</param>
        /// <param name="color">color of the sphere</param>
        /// <param name="duration">how long the sphere will be visible for in seconds</param>
        /// <param name="depthTest">does not perform depth tests if <c>false</c></param>
        public static void DrawWireframeSphere(
            Vector3 position,
            Quaternion rotation = default,
            Vector3 scale = default,
            uint segmentCount = DefaultSegmentCount,
            Color color = default,
            float duration = 0f,
            bool depthTest = true)
        {
#if DEBUG
            if (rotation == default)
                rotation = Quaternion.identity;

            if (scale == default)
            {
                scale = Vector3.one;
            }
            else if (scale.sqrMagnitude == 0f)
            {
                return;
            }

            if (segmentCount < 1)
                return;

            if (color == default)
            {
                color = Color.white;
            }
            else if (color.a <= 0f)
            {
                return;
            }

            var xCircleScale = new Vector2(scale.y, scale.z);
            var yCircleScale = new Vector2(scale.x, scale.z);
            var zCircleScale = new Vector2(scale.x, scale.y);
            var xCircleRotation = rotation * RightRotation;
            var yCircleRotation = rotation;
            var zCircleRotation = rotation * ForwardRotation;
            DrawWireframeCircle(position, xCircleRotation, xCircleScale, segmentCount, color, duration, depthTest);
            DrawWireframeCircle(position, yCircleRotation, yCircleScale, segmentCount, color, duration, depthTest);
            DrawWireframeCircle(position, zCircleRotation, zCircleScale, segmentCount, color, duration, depthTest);
#endif
        }

        /// <summary>
        /// Draws a wireframe circle.
        /// </summary>
        /// <param name="position">center of the circle</param>
        /// <param name="rotation">rotation of the circle around the <paramref name="position"/></param>
        /// <param name="scale">scale of the circle, independent of <paramref name="rotation"/></param>
        /// <param name="segmentCount">number of segments on the circle</param>
        /// <param name="color">color of the circle</param>
        /// <param name="duration">how long the circle will be visible for in seconds</param>
        /// <param name="depthTest">does not perform depth tests if <c>false</c></param>
        public static void DrawWireframeCircle(
            Vector3 position,
            Quaternion rotation = default,
            Vector2 scale = default,
            uint segmentCount = DefaultSegmentCount,
            Color color = default,
            float duration = 0f,
            bool depthTest = true)
        {
#if DEBUG
            if (rotation == default)
                rotation = Quaternion.identity;

            if (scale == default)
            {
                scale = Vector2.one;
            }
            else if (scale.sqrMagnitude <= 0f)
            {
                return;
            }

            if (segmentCount < 1)
                return;

            if (color == default)
            {
                color = Color.white;
            }
            else if (color.a <= 0f)
            {
                return;
            }
            
            // `rsMatrix` is a transformation that can be calculated once ahead of time.
            // This saves multiplications per vertex.
            var rotationMatrix = Matrix4x4.Rotate(rotation);
            var scaleMatrix = Matrix4x4.Scale(new Vector3(scale.x, 1f, scale.y));
            var rsMatrix = rotationMatrix * scaleMatrix;
            
            // The partition multiplier can be multiplied with a segment to get the vertex angle.
            var partitionMultiplier = 360f / segmentCount;
            
            // `fromPosition` always contains the "previous" `toPosition` and is calculated at the start.
            // This is because every `toPosition` is the `fromPosition` of the next segment.
            // By overriding this variable, the multiplications can be halved.
            var fromPosition = position + GetPartitionTranslate(rsMatrix, 0f);
            
            for (var segment = 1; segment <= segmentCount; segment++)
            {
                var vertexAngleDeg = partitionMultiplier * segment;
                var toPosition = position + GetPartitionTranslate(rsMatrix, vertexAngleDeg);
                UnityEngine.Debug.DrawLine(fromPosition, toPosition, color, duration, depthTest);
                fromPosition = toPosition;
            }
#endif
        }

#if DEBUG
        private static Vector3 GetPartitionTranslate(Matrix4x4 rsMatrix, float vertexAngleDeg)
        {
            var vertexAngleRad = Mathf.Deg2Rad * vertexAngleDeg;
            var translate = new Vector3(Mathf.Cos(vertexAngleRad), 0f, Mathf.Sin(vertexAngleRad));
            var rst = rsMatrix * translate;
            return new Vector3(rst.x, rst.y, rst.z);
        }
#endif
    }
}
