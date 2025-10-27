using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class MotionSplineMatcher : MonoBehaviour
{
    [Header("Inputs")]
    public Transform controller;
    public SplineContainer targetSpline;

    [Header("Capture")]
    public float sampleHz = 60f;
    public float bufferSeconds = 2f;
    public Transform frameRoot;

    [Header("Comparison")]
    public int resamplePoints = 64;
    public bool useDTW = true;
    public float passThreshold = 0.78f;
    public float maxNormalizedDist = 0.6f;

    [Header("Output")]
    public UnityEvent onPass;
    public UnityEvent<float> onScore;

    readonly List<Vector3> samples = new List<Vector3>();
    readonly List<float> times = new List<float>();
    float accum;
    float dt;
    Vector3[] refSeq;

    void Awake()
    {
        dt = 1f / Mathf.Max(1f, sampleHz);
        if (frameRoot == null) frameRoot = transform;
        refSeq = SplineSequence.BuildRefSequence(targetSpline, resamplePoints);
        SequenceUtils.NormalizeInPlace(ref refSeq);
    }

    void Update()
    {
        if (controller == null || targetSpline == null) return;

        accum += Time.deltaTime;
        while (accum >= dt)
        {
            accum -= dt;
            var p = frameRoot.InverseTransformPoint(controller.position);
            samples.Add(p);
            times.Add(Time.time);
        }

        float cutoff = Time.time - bufferSeconds;
        while (times.Count > 0 && times[0] < cutoff)
        {
            times.RemoveAt(0);
            samples.RemoveAt(0);
        }

        if (samples.Count < 8) return;

        var liveSeq = SequenceUtils.ResampleSequence(samples, resamplePoints);
        SequenceUtils.NormalizeInPlace(ref liveSeq);

        if (refSeq == null || refSeq.Length != resamplePoints)
        {
            refSeq = SplineSequence.BuildRefSequence(targetSpline, resamplePoints);
            SequenceUtils.NormalizeInPlace(ref refSeq);
        }

        float dist = useDTW ? DistanceMetrics.DTW(liveSeq, refSeq) : DistanceMetrics.DiscreteFrechet(liveSeq, refSeq);
        float score = Mathf.Clamp01(1f - (dist / Mathf.Max(1e-5f, maxNormalizedDist)));
        onScore?.Invoke(score);
        if (score >= passThreshold) onPass?.Invoke();
    }
}
