using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class DancePoseSequencer : MonoBehaviour
{
    [System.Serializable]
    public class Pose { public Vector3 left; public Quaternion leftRot; public Vector3 right; public Quaternion rightRot; }
    [System.Serializable]
    public class PoseList { public List<Pose> poses = new List<Pose>(); }

    public Transform xrOrigin;
    public Transform head;
    public Transform leftController;
    public Transform rightController;
    public string loadFileName = "Poses.json";
    public float positionTolerance = 0.25f;
    public float rotationToleranceDegrees = 25f;
    public float holdSeconds = 0.15f;
    public bool loop = true;

    public bool visualize = true;
    public float ghostScale = 0.06f;
    public Material activeMat;

    List<Pose> poses = new List<Pose>();
    int index;
    float holdTimer;

    Transform leftGhost;
    Transform rightGhost;

    Matrix4x4 GetHeadSpace(out Quaternion basisRot)
    {
        var pos = new Vector3(head.position.x, xrOrigin.position.y, head.position.z);
        basisRot = Quaternion.Euler(0f, head.eulerAngles.y, 0f);
        return Matrix4x4.TRS(pos, basisRot, Vector3.one);
    }

    void Start()
    {
        var path = Path.Combine(Application.persistentDataPath, loadFileName);
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var list = JsonUtility.FromJson<PoseList>(json);
            poses = list?.poses ?? new List<Pose>();
        }

        index = 0;
        holdTimer = 0f;

        if (visualize) BuildGhosts();
        UpdateGhostVisibility();
    }

    void Update()
    {
        if (poses.Count == 0) return;

        Quaternion basisRot;
        var H = GetHeadSpace(out basisRot);

        if (visualize) UpdateGhostTransforms(H, basisRot);

        var target = poses[index];

        var lPosLocal = H.inverse.MultiplyPoint3x4(leftController.position);
        var rPosLocal = H.inverse.MultiplyPoint3x4(rightController.position);
        var lRotLocal = Quaternion.Inverse(basisRot) * leftController.rotation;
        var rRotLocal = Quaternion.Inverse(basisRot) * rightController.rotation;

        float lp = Vector3.Distance(lPosLocal, target.left);
        float rp = Vector3.Distance(rPosLocal, target.right);
        float lr = Quaternion.Angle(lRotLocal, target.leftRot);
        float rr = Quaternion.Angle(rRotLocal, target.rightRot);

        bool match = lp <= positionTolerance && rp <= positionTolerance && lr <= rotationToleranceDegrees && rr <= rotationToleranceDegrees;

        if (match)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdSeconds)
            {
                index++;
                holdTimer = 0f;
                if (index >= poses.Count)
                {
                    if (loop) index = 0;
                    else enabled = false;
                }
                UpdateGhostVisibility();
            }
        }
        else holdTimer = 0f;
    }

    void BuildGhosts()
    {
        leftGhost = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        rightGhost = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        leftGhost.name = "ActiveLeftGhost";
        rightGhost.name = "ActiveRightGhost";
        leftGhost.localScale = Vector3.one * ghostScale;
        rightGhost.localScale = Vector3.one * ghostScale;
        if (activeMat)
        {
            leftGhost.GetComponent<Renderer>().sharedMaterial = activeMat;
            rightGhost.GetComponent<Renderer>().sharedMaterial = activeMat;
        }
        Destroy(leftGhost.GetComponent<Collider>());
        Destroy(rightGhost.GetComponent<Collider>());
    }

    void UpdateGhostTransforms(Matrix4x4 H, Quaternion basisRot)
    {
        if (index >= poses.Count) return;
        var p = poses[index];
        leftGhost.position = H.MultiplyPoint3x4(p.left);
        leftGhost.rotation = basisRot * p.leftRot;
        rightGhost.position = H.MultiplyPoint3x4(p.right);
        rightGhost.rotation = basisRot * p.rightRot;
    }

    void UpdateGhostVisibility()
    {
        if (!visualize) return;
        if (index < poses.Count)
        {
            leftGhost.gameObject.SetActive(true);
            rightGhost.gameObject.SetActive(true);
        }
        else
        {
            leftGhost.gameObject.SetActive(false);
            rightGhost.gameObject.SetActive(false);
        }
    }
}
