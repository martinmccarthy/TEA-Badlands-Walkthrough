using System;
using System.Collections.Generic;
using UnityEngine;

public static class SequenceUtils
{
    public static Vector3[] ResampleSequence(List<Vector3> seq, int n)
    {
        if (seq.Count == 0) return Array.Empty<Vector3>();
        var cum = new float[seq.Count];
        cum[0] = 0f;
        for (int i = 1; i < seq.Count; i++) cum[i] = cum[i - 1] + Vector3.Distance(seq[i - 1], seq[i]);
        float total = cum[cum.Length - 1];
        if (total < 1e-6f)
        {
            var flat = new Vector3[n];
            for (int i = 0; i < n; i++) flat[i] = seq[0];
            return flat;
        }
        var outSeq = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            float d = (total * i) / (n - 1);
            int idx = Array.BinarySearch(cum, d);
            if (idx < 0) idx = ~idx;
            if (idx <= 0) outSeq[i] = seq[0];
            else if (idx >= cum.Length) outSeq[i] = seq[^1];
            else
            {
                float t = Mathf.InverseLerp(cum[idx - 1], cum[idx], d);
                outSeq[i] = Vector3.Lerp(seq[idx - 1], seq[idx], t);
            }
        }
        return outSeq;
    }

    public static void NormalizeInPlace(ref Vector3[] seq)
    {
        if (seq.Length == 0) return;
        var centroid = Vector3.zero;
        for (int i = 0; i < seq.Length; i++) centroid += seq[i];
        centroid /= seq.Length;
        for (int i = 0; i < seq.Length; i++) seq[i] -= centroid;

        float len = 0f;
        for (int i = 1; i < seq.Length; i++) len += Vector3.Distance(seq[i - 1], seq[i]);
        float s = len > 1e-6f ? 1f / len : 1f;
        for (int i = 0; i < seq.Length; i++) seq[i] *= s;

        if (seq.Length >= 2)
        {
            var a = seq[seq.Length - 1] - seq[0];
            var yawA = new Vector2(a.x, a.z);
            if (yawA.sqrMagnitude > 1e-8f)
            {
                var rot = Quaternion.FromToRotation(new Vector3(1, 0, 0), new Vector3(yawA.x, 0, yawA.y));
                for (int i = 0; i < seq.Length; i++) seq[i] = rot * seq[i];
            }
        }
    }
}
