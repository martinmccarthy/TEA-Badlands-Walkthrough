using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class RuntimeSplineRecorder : MonoBehaviour
{
    [Header("Source")]
    public Transform controller;
    public Transform frameRoot;

    [Header("Capture")]
    public float sampleHz = 72f;
    public float minPointDelta = 0.02f;
    public bool holdToRecord = true;
    public InputActionReference recordAction;

    [Header("Spline Output")]
    public SplineContainer outputContainer;
    public string outputName = "RecordedSpline";
    public bool closeSpline = false;
    public float tangentScale = 0.33f;

    [Header("Filtering")]
    public float simplifyEpsilon = 0.005f;

    readonly List<Vector3> _points = new();
    float _dt;
    float _accum;
    bool _recording;
    Vector3 _lastAdded;

    void OnEnable()
    {
        _dt = 1f / Mathf.Max(1f, sampleHz);
        if (recordAction != null)
        {
            recordAction.action.Enable();
            recordAction.action.performed += OnPerformed;
            recordAction.action.canceled += OnCanceled;
        }
        if (frameRoot == null) frameRoot = transform;
    }

    void OnDisable()
    {
        if (recordAction != null)
        {
            recordAction.action.performed -= OnPerformed;
            recordAction.action.canceled -= OnCanceled;
            recordAction.action.Disable();
        }
    }

    void Update()
    {
        if (controller == null) return;
        if (!holdToRecord && recordAction != null && recordAction.action.WasPressedThisFrame())
            _recording = !_recording;

        if (!_recording) return;

        _accum += Time.deltaTime;
        while (_accum >= _dt)
        {
            _accum -= _dt;
            var p = frameRoot.InverseTransformPoint(controller.position);
            if (_points.Count == 0 || (p - _lastAdded).sqrMagnitude >= (minPointDelta * minPointDelta))
            {
                _points.Add(p);
                _lastAdded = p;
            }
        }
    }

    void OnPerformed(InputAction.CallbackContext ctx)
    {
        if (holdToRecord)
        {
            StartRecording();
        }
    }

    void OnCanceled(InputAction.CallbackContext ctx)
    {
        if (holdToRecord)
        {
            StopAndBake();
        }
    }

    public void StartRecording()
    {
        _points.Clear();
        _accum = 0f;
        _recording = true;
    }

    public void StopAndBake()
    {
        _recording = false;
        if (_points.Count < 2) return;

        var simplified = RdpSimplify(_points, simplifyEpsilon);
        if (simplified.Count < 2) simplified = new List<Vector3>(_points);

        if (outputContainer == null)
        {
            var go = new GameObject(outputName);
            go.transform.SetParent(frameRoot, false);
            outputContainer = go.AddComponent<SplineContainer>();
        }

        var spline = outputContainer.Spline;
        spline.Clear();
        var knots = BuildKnots(simplified, tangentScale, closeSpline);
        spline.Knots = knots;
        spline.Closed = closeSpline;

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(outputContainer);
#endif
    }

    static BezierKnot[] BuildKnots(List<Vector3> pts, float scale, bool closed)
    {
        int n = pts.Count;
        var knots = new BezierKnot[n];
        for (int i = 0; i < n; i++)
        {
            Vector3 p = pts[i];
            Vector3 pPrev = pts[(i - 1 + n) % n];
            Vector3 pNext = pts[(i + 1) % n];

            bool hasPrev = closed || i > 0;
            bool hasNext = closed || i < n - 1;

            Vector3 m = Vector3.zero;
            if (hasPrev && hasNext)
                m = 0.5f * (pNext - pPrev);
            else if (hasPrev)
                m = (p - pPrev);
            else if (hasNext)
                m = (pNext - p);

            float localScale = scale * Mathf.Min((p - pPrev).magnitude, (pNext - p).magnitude);
            Vector3 tOut = m.normalized * localScale;
            Vector3 tIn = -tOut;

            knots[i] = new BezierKnot(p, tIn, tOut, Quaternion.identity);
        }
        return knots;
    }

    static List<Vector3> RdpSimplify(List<Vector3> pts, float epsilon)
    {
        if (pts.Count < 3 || epsilon <= 0f) return new List<Vector3>(pts);
        var keep = new bool[pts.Count];
        keep[0] = keep[^1] = true;
        SimplifySection(pts, 0, pts.Count - 1, epsilon, keep);
        var outPts = new List<Vector3>(pts.Count);
        for (int i = 0; i < pts.Count; i++) if (keep[i]) outPts.Add(pts[i]);
        return outPts;
    }

    static void SimplifySection(List<Vector3> pts, int start, int end, float eps, bool[] keep)
    {
        if (end <= start + 1) return;
        float maxDist = 0f;
        int index = -1;
        Vector3 a = pts[start], b = pts[end];
        for (int i = start + 1; i < end; i++)
        {
            float d = PointLineDistance(pts[i], a, b);
            if (d > maxDist)
            {
                maxDist = d;
                index = i;
            }
        }
        if (maxDist > eps && index != -1)
        {
            keep[index] = true;
            SimplifySection(pts, start, index, eps, keep);
            SimplifySection(pts, index, end, eps, keep);
        }
    }

    static float PointLineDistance(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = ab.sqrMagnitude < 1e-8f ? 0f : Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        Vector3 proj = a + t * ab;
        return Vector3.Distance(p, proj);
    }

    void OnDrawGizmosSelected()
    {
        if (_points.Count < 2) return;
        Gizmos.matrix = frameRoot ? frameRoot.localToWorldMatrix : Matrix4x4.identity;
        for (int i = 1; i < _points.Count; i++)
            Gizmos.DrawLine(_points[i - 1], _points[i]);
    }
}
