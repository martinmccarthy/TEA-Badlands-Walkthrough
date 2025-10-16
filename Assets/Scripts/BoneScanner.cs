using System.Collections.Generic;
using UnityEngine;

public class BoneScanner : MonoBehaviour
{

    [SerializeField] private List<GameObject> m_bones;
    [SerializeField] private GameObject screen;
    [SerializeField] private List<Material> m_materials;
    private void OnTriggerEnter(Collider other)
    {
        if(m_bones.Find(obj => obj.name == other.name)) UpdateScreen(other.name);
    }

    private void OnTriggerExit(Collider other)
    {
        if(m_bones.Find(obj => obj.name == other.name)) UpdateScreen("blank");
    }

    void UpdateScreen(string bone)
    {
        Renderer r = screen.GetComponent<Renderer>();
        r.material = m_materials.Find(mat => mat.name == $"{bone}Mat");
    }
}