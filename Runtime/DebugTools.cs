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
            var a = new Vector3(halfScale.x, 0f, halfScale.y);
            var b = new Vector3(halfScale.x, 0f, -halfScale.y);
            
            var corners = new[]
            {
                position + rotation * -b,
                position + rotation * -a,
                position + rotation * b,
                position + rotation * a
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
            _ = DrawSegmentedLineInternal(
                start,
                end,
                segmentLength,
                segmentSpacing,
                segmentColor,
                0f,
                timeScale,
                depthTest);
        }

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
#if DEBUG
            if (segmentColor == default)
            {
                segmentColor = Color.white;
            }
            else if (segmentColor.a == 0f)
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
#endif
        }

#if DEBUG
        private static void DrawSegment(Vector3 start, Vector3 end, Color segmentColor, bool depthTest)
        {
            UnityEngine.Debug.DrawLine(start, end, segmentColor, 0f, depthTest);
        }
#endif
    }
}
