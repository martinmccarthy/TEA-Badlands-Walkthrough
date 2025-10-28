using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenChanger : MonoBehaviour
{
    [SerializeField]
    private List<Material> materials = new List<Material>();

    public void SwapImage(string name) // could do logic here but kind of lazy
    {
        Material mat = materials.Find(x => x.name == name);
        gameObject.GetComponent<Renderer>().material = mat;
    }
}
