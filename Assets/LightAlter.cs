using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightAlter : MonoBehaviour
{
    // can try to do a gradient for this, fading from black to the color but i might pass on that
    // seems like work i dont want to do
    public void ChangeColor(Color c)
    {
        Light l = gameObject.GetComponent<Light>();
        l.color = c;
    }
}
