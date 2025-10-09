using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceController : MonoBehaviour
{
    [SerializeField] private PlayerController m_playerController;

    private bool checkingForDance = false;
    private List<DanceMove> danceMoves = new();
    private int currentDanceMoveIndex = 0;
    
    private void Update()
    {
        if(checkingForDance)
        {
            DanceMove current = danceMoves[currentDanceMoveIndex];
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
