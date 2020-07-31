using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{

    [Serializable]
    public class ColorState
    {
        public Color BaseColor;
        public Color EmissionColor;
    }

    [Serializable]
    public class MaterialColor
    {
        public string MaterialName;
        public Material MeshMaterial;
        public List<ColorState> States = new List<ColorState>();
    }

    public SkinnedMeshRenderer SkinMesh;
    [Min(0f)]
    public int MeshColorState = 0;
    public float ColorChangeSpeed = 1f;
    public bool updatingColorStates = false;
    public List<Material> mats = new List<Material>();
    public List<MaterialColor> Colors = new List<MaterialColor>();

    private int currentColorState = 0;
    // Start is called before the first frame update
    void Start()
    {
        mats = SkinMesh.materials.ToList();
        foreach (Material mat in mats)
        {
            foreach (MaterialColor matColor in Colors)
            {
                if (matColor.MaterialName + " (Instance)" == mat.name)
                {
                    matColor.MeshMaterial = mat;// SkinMesh.materials[mats.IndexOf(mat)];
                    continue;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(currentColorState != MeshColorState)
        {
            updatingColorStates = true;
            currentColorState = MeshColorState;
        }
        if (updatingColorStates)
            updateColorStates();
    }

    public void SetMeshColorState(int state)
    {
        MeshColorState = state;
    }

    private void updateColorStates()
    {
        int ColorCount = Colors.Count;
        foreach (MaterialColor matColor in Colors)
        {
            //Get the base and emissive colors of the material
            Color materialColor = matColor.MeshMaterial.color;
            Color emissionColor = matColor.MeshMaterial.GetColor("_EmissionColor");

            //Modify the new color references with a lerp towards their target colors
            materialColor = Color.Lerp(materialColor, matColor.States[MeshColorState].BaseColor,
                Time.deltaTime * ColorChangeSpeed);
            emissionColor = Color.Lerp(emissionColor, matColor.States[MeshColorState].EmissionColor,
                Time.deltaTime * ColorChangeSpeed);

            //If the colors are identical, take a hit off the counter so it can stop updating if it's zero
            if (materialColor == matColor.States[MeshColorState].BaseColor &&
                emissionColor == matColor.States[MeshColorState].EmissionColor)
                ColorCount--;

            //Set the new colors back into the material
            matColor.MeshMaterial.color = materialColor;
            matColor.MeshMaterial.SetColor("_EmissionColor", emissionColor);
        }
        if (ColorCount <= 0)
            updatingColorStates = false;
    }
}
