using UnityEngine;
using UnityEngine.XR;

public class DeviceInfo
{
    public InputDevice Device;
    public Vector3 Position;
    public Quaternion Rotation;
    public DeviceInfo(InputDevice device, Vector3 position, Quaternion rotation)
    {
        Device = device;
        Position = position;
        Rotation = rotation;
    }
}