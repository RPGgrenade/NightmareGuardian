using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleHandler : MonoBehaviour
{
    [Serializable]
    public class Particles
    {
        [Header("Properties")]
        [Tooltip("ID in case needed")]
        public int ID = 0;
        [Tooltip("Name in case needed")]
        public string Name;
        [Tooltip("Determines whether the particle system is a child of the entity")]
        public bool IsChild;
        [Tooltip("Determines what the particle system to spawn is")]
        public GameObject Particle;

        [Header("Positioning")]
        [Tooltip("Determines the position parent of the particle system")]
        public Transform Position;
        [Tooltip("Offset from base position")]
        public Vector3 PositionOffset;
        [Tooltip("Rotation in base position")]
        public Vector3 RotationOffset;
        [Tooltip("Scale in position")]
        public float ScaleDifference = 1f;

        [Header("Skinned Mesh Options")]
        [Tooltip("Whether or not to use a skin mesh as part of the particle")]
        public bool UseSkinnedMesh = false;
        [Tooltip("The renderer to be assigned (only null if not using skinned renderer)")]
        public SkinnedMeshRenderer Renderer;
        [Tooltip("Whether or not the particles will reflect the texture colors")]
        public bool UseSkinColors = false;
    }

    public List<Particles> ParticleList = new List<Particles>();
    
    public void FindSpawnParticle(int id)
    {
        foreach (Particles part in ParticleList)
        {
            if (part.ID == id)
            {
                createParticleSystem(part);
                break;
            }
        }
    }

    public void FindSpawnParticle(string name)
    {
        foreach (Particles part in ParticleList)
        {
            if (part.Name == name)
            {
                createParticleSystem(part);
                break;
            }
        }
    }

    public void SpawnParticle(GameObject particle)
    {
        foreach (Particles part in ParticleList)
        {
            if (part.Particle == particle)
            {
                createParticleSystem(part);
                break;
            }
        }
    }

    void createParticleSystem(Particles part)
    {
        GameObject particles;
        if (!part.IsChild)
            particles = GameObject.Instantiate(part.Particle,
                part.Position.position + part.PositionOffset, 
                part.Position.rotation);
        else
            particles = GameObject.Instantiate(part.Particle,
                part.Position.position + part.PositionOffset, 
                part.Position.rotation,
                part.Position);
        particles.transform.Rotate(part.RotationOffset, Space.Self);
        if (part.IsChild)
        {
            particles.transform.localPosition = part.PositionOffset;
            particles.transform.localScale = particles.transform.localScale * part.ScaleDifference;
        }
        else
            particles.transform.localScale = particles.transform.lossyScale * part.ScaleDifference;

        if (part.UseSkinnedMesh)
        {
            ParticleSkinSetter setter = particles.GetComponent<ParticleSkinSetter>();
            setter.Renderer = part.Renderer;
            setter.UseMeshColors = part.UseSkinColors;
            setter.SetRenderer();
        }
    }
}
