using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonarManager : MonoBehaviour {

    public MeshRenderer RendererMesh;
    public float ExpansionSpeed = 1f;
    public float SizeModifier = 1f;
    public float MaxSize = 1f;

    private float size = 0f;
    private float r = 0f;
    private float b = 0f;
    private float g = 0f;
	// Use this for initialization
	void Start () {
        if (RendererMesh == null)
            RendererMesh = this.GetComponent<MeshRenderer>();
        Color color = RendererMesh.material.GetColor("_RimColor");
        r = color.r;
        b = color.b;
        g = color.g;
        Transform mainCam = Camera.main.transform;
        float distance = findDistanceFromCamera(mainCam);
        mainCam.GetComponent<CameraShake>().ShakeCamera(distance, SizeModifier, MaxSize);
    }

    private float findDistanceFromCamera(Transform cam)
    {
        return Vector3.Distance(cam.position, this.transform.position);
    }

    public void SetProperties(float sizes, float speed = 1f, float size = 1f)
    {
        ExpansionSpeed = speed;
        SizeModifier = size;
        MaxSize = sizes;
    }
	
	// Update is called once per frame
	void Update () {
        increaseSize();
        checkDeath();
	}

    private void checkDeath()
    {
        if (size >= MaxSize)
            GameObject.Destroy(this.gameObject);
    }

    private void increaseSize()
    {
        size += Time.deltaTime * MaxSize * ExpansionSpeed;
        this.transform.localScale = Vector3.one * size * SizeModifier;
        fading();
    }

    void fading()
    {
        Color color = RendererMesh.material.GetColor("_RimColor");
        float percent = (MaxSize - size) / MaxSize;
        color.r = r * percent;
        color.b = b * percent;
        color.g = g * percent;
        RendererMesh.material.SetColor("_RimColor", color);
    }
}
