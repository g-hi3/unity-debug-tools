using UnityEngine;

namespace G_hi3.Debug
{
    /// <summary>
    /// Provides custom debug utility methods.
    /// </summary>
    public static class DebugTools
    {
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
        /// <para>
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
            float segmentLength = 0.2f,
            float segmentSpacing = 0.1f,
            Color segmentColor = default,
            float timeScale = 0f,
            bool depthTest = true)
        {
#if DEBUG
            if (segmentColor == default)
            {
                segmentColor = Color.white;
            }
            else if (segmentColor.a == 0f)
            {
                return;
            }

            if (segmentLength <= 0f)
                return;
            
            if (segmentSpacing <= 0f)
            {
                DrawSegment(start, end, segmentColor, depthTest);
                return;
            }

            var delta = end - start;
            var deltaLength = delta.magnitude;
            var direction = delta.normalized;
            var unitLength = segmentLength + segmentSpacing;
            var offset = Time.time * timeScale % unitLength;
            var distanceTravelled = offset;

            if (distanceTravelled < 0f)
            {
                var n = (int)(-distanceTravelled % unitLength);
                distanceTravelled += n * unitLength;
                
                if (segmentLength > -distanceTravelled)
                {
                    var offsetSegmentLength = distanceTravelled + segmentLength;
                    var segmentEnd = start + offsetSegmentLength * direction;
                    DrawSegment(start, segmentEnd, segmentColor, depthTest);
                }

                distanceTravelled += unitLength;
            }
            else if (distanceTravelled > 0f)
            {
                var offsetSegmentLength = distanceTravelled - segmentSpacing;
                
                if (offsetSegmentLength > 0)
                {
                    var segmentEnd = start + offsetSegmentLength * direction;
                    DrawSegment(start, segmentEnd, segmentColor, depthTest);
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
