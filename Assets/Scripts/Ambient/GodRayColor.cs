using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GodRayColor : MonoBehaviour
{
    public Color color;

    private new MeshRenderer renderer;

    void Start()
    {
        renderer = this.GetComponent<MeshRenderer>();
        renderer.material.SetColor("_TintColor", color);
    }
}
