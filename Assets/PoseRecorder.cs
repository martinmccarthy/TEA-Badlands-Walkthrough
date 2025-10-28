using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class PoseRecorder : MonoBehaviour
{
    [System.Serializable]
    public class Pose { public Vector3 left; public Quaternion leftRot; public Vector3 right; public Quaternion rightRot; }
    [System.Serializable]
    public class PoseList { public List<Pose> poses = new List<Pose>(); }

    public Transform xrOrigin;
    public Transform head;
    public Transform leftController;
    public Transform rightController;
    public string saveFileName = "Poses.json";

    PoseList data = new PoseList();

    Matrix4x4 GetHeadSpace(out Quaternion basisRot)
    {
        var pos = new Vector3(head.position.x, xrOrigin.position.y, head.position.z);
        basisRot = Quaternion.Euler(0f, head.eulerAngles.y, 0f);
        return Matrix4x4.TRS(pos, basisRot, Vector3.one);
    }

    public void Record()
    {
        Quaternion basisRot;
        var H = GetHeadSpace(out basisRot);
        var Hinv = H.inverse;

        var p = new Pose
        {
            left = Hinv.MultiplyPoint3x4(leftController.position),
            right = Hinv.MultiplyPoint3x4(rightController.position),
            leftRot = Quaternion.Inverse(basisRot) * leftController.rotation,
            rightRot = Quaternion.Inverse(basisRot) * rightController.rotation
        };

        data.poses.Add(p);
        var path = Path.Combine(Application.persistentDataPath, saveFileName);
        File.WriteAllText(path, JsonUtility.ToJson(data, true));
        Debug.Log($"Saved {data.poses.Count} poses -> {path}");
    }
}
