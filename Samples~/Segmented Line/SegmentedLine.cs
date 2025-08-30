using G_hi3.Debug;
using UnityEngine;

public sealed class SegmentedLine : MonoBehaviour
{
    [SerializeField]
    private Transform start;

    [SerializeField]
    private Transform end;

    [SerializeField]
    private Color segmentColor;

    [SerializeField]
    private float segmentLength;

    [SerializeField]
    private float segmentSpacing;

    [SerializeField]
    private float timeScale;

    [SerializeField]
    private bool depthTest;

    private void Update()
    {
        if (start?.position is { } startPosition
            && end?.position is { } endPosition)
        {
            DebugTools.DrawSegmentedLine(
                startPosition,
                endPosition,
                segmentLength,
                segmentSpacing,
                segmentColor,
                timeScale,
                depthTest);
        }
    }

    private void Reset()
    {
        start = null;
        end = null;
        segmentColor = Color.green;
        segmentLength = 0.2f;
        segmentSpacing = 0.1f;
        timeScale = 1f;
        depthTest = true;
    }
}
