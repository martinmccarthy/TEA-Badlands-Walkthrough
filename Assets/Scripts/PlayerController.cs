using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputDevice m_leftController;
    [SerializeField] private InputDevice m_rightController;
    [SerializeField] private InputDevice m_headset;

    // this is TRASH code but idc
    public List<DeviceInfo> GetPositions()
    {
        List<DeviceInfo> deviceInfo = new();
        GetPositionAndRotation(m_headset, out Vector3 headsetPos, out Quaternion headsetRot);
        deviceInfo.Add(new DeviceInfo(m_headset, headsetPos, headsetRot));
        GetPositionAndRotation(m_leftController, out Vector3 leftPos, out Quaternion leftRot);
        deviceInfo.Add(new DeviceInfo(m_leftController, leftPos, leftRot));
        GetPositionAndRotation(m_rightController, out Vector3 rightPos, out Quaternion rightRot);
        deviceInfo.Add(new DeviceInfo(m_rightController, rightPos, rightRot));

        return deviceInfo;
    }

    private void GetPositionAndRotation(InputDevice device, out Vector3 position, out Quaternion rotation)
    {
        device.TryGetFeatureValue(CommonUsages.devicePosition, out position);
        device.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation);
        Debug.Log($"Device {device.name}:\n Position: {position}, Rotation: {rotation}");
    }
}
