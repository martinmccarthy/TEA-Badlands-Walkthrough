using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceController : MonoBehaviour
{
    [SerializeField] private PlayerController m_playerController;

    private bool checkingForDance = false;
    private Queue<DanceMove> danceMoves = new();
    
    private void Start()
    {
        InitializeDanceQueue();
    }

    private void InitializeDanceQueue()
    {
        DanceMove initial = new(
            new Vector3(0.5f, 1.0f, 0.5f), Quaternion.Euler(0, 0, 0),
            new Vector3(-0.5f, 1.0f, 0.5f), Quaternion.Euler(0, 0, 0),
            0.2f, 15f, 5f
        );
        danceMoves.Enqueue(initial);
    }

    private void Update()
    {
        if(checkingForDance)
        {
            DanceMove current = danceMoves.Peek();
            List<DeviceInfo> deviceInfo = m_playerController.GetPositions();
            DeviceInfo leftController = deviceInfo.Find(d => d.Device.characteristics.HasFlag(UnityEngine.XR.InputDeviceCharacteristics.Left));
            DeviceInfo rightController = deviceInfo.Find(d => d.Device.characteristics.HasFlag(UnityEngine.XR.InputDeviceCharacteristics.Right));
            bool movePerformed = current.CheckMove(leftController.Position, leftController.Rotation, rightController.Position, rightController.Rotation);
            if(movePerformed)
            {
                Debug.Log("Dance move performed!");
                danceMoves.Dequeue();
                if(danceMoves.Count == 0)
                {
                    checkingForDance = false;
                }
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            checkingForDance = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            checkingForDance = false;
        }
    }
}
