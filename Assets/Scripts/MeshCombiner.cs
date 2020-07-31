using System.Collections;
using UnityEngine;

public class MeshCombiner : MonoBehaviour {

	public void CombineMeshes()
    {
        Quaternion oldRot = transform.rotation;
        Vector3 oldPos = transform.position;

        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;

        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>();

        Debug.Log(name + " is combining" + filters.Length + "Meshes");

        Mesh finaleMesh = new Mesh();
        
        CombineInstance[] combiners = new CombineInstance[filters.Length];

        for(int i = 0; i < filters.Length; i++)
        {
            if (filters[i].transform == transform)
                continue;
            combiners[i].subMeshIndex = 0;
            combiners[i].mesh = filters[i].sharedMesh;
            combiners[i].transform = filters[i].transform.localToWorldMatrix;
        }

        finaleMesh.CombineMeshes(combiners);

        GetComponent<MeshFilter>().sharedMesh = finaleMesh;

        transform.rotation = oldRot;
        transform.position = oldPos;

        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
