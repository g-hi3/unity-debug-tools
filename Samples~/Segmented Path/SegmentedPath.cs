using G_hi3.Debug;
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SegmentedPath : MonoBehaviour
{
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

    [SerializeField]
    private bool loop;

    [SerializeField]
    private Transform[] pathPoints;

    private void Update()
    {
        var positions = GetPositions();
        
        DebugTools.DrawSegmentedPath(
            positions,
            segmentLength,
            segmentSpacing,
            segmentColor,
            timeScale,
            depthTest,
            loop);
    }

    private Vector3[] GetPositions()
    {
        if (pathPoints == null)
            return Array.Empty<Vector3>();
        
        var positions = new List<Vector3>();
        
        foreach (var pathPoint in pathPoints)
        {
            if (pathPoint != null)
                positions.Add(pathPoint.position);
        }

        return positions.ToArray();
    }

    private void Reset()
    {
        segmentColor = Color.cyan;
        segmentLength = 0.2f;
        segmentSpacing = 0.1f;
        timeScale = 1f;
        depthTest = true;
        loop = false;
        pathPoints = Array.Empty<Transform>();
    }
}
