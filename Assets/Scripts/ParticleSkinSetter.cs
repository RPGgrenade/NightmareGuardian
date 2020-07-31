using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSkinSetter : MonoBehaviour
{
    public ParticleSystem Particles;
    public SkinnedMeshRenderer Renderer;
    public bool UseMeshColors = true;

    private bool RendererSet = false;
    // Start is called before the first frame update
    void Start()
    {
        if (Particles == null)
            Particles = this.GetComponent<ParticleSystem>();
        if (Renderer == null)
            getRenderer();
    }

    void getRenderer()
    {
        Debug.Log("Problem finding Renderer");
        SkinnedMeshRenderer[] rendererList = 
            this.transform.parent.GetComponentsInChildren<SkinnedMeshRenderer>();
        if(rendererList.Length > 0)
            Renderer = rendererList[0]; //Set skinned mesh renderer to first renderer in list (may be random)
    }

    public void SetRenderer()
    {
        ParticleSystem.ShapeModule shape = Particles.shape;
        shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
        shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
        shape.useMeshColors = UseMeshColors;
        shape.skinnedMeshRenderer = Renderer;
        RendererSet = shape.skinnedMeshRenderer == Renderer;
        Debug.Log("Skinned Mesh: " + shape.skinnedMeshRenderer.name);
    }

    // Update is called once per frame
    void Update()
    {
        //if (!RendererSet)
        //{
        //    if (Renderer == null)
        //        getRenderer();
        //    else
                SetRenderer();
        //}
    }
}
