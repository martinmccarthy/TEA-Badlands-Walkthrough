using UnityEngine;
using UnityEngine.Splines;

public static class SplineSequence
{
    public static Vector3[] BuildRefSequence(SplineContainer container, int resamplePoints)
    {
        var seq = new Vector3[resamplePoints];
        var spline = container.Spline;
        for (int i = 0; i < resamplePoints; i++)
        {
            float t = i / (float)(resamplePoints - 1);
            seq[i] = container.transform.TransformPoint(spline.EvaluatePosition(t));
        }
        return seq;
    }
}
