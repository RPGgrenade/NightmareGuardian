using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringRender : MonoBehaviour
{
    [Header("Targets")]
    public Transform[] StringPath;

    public Transform TargetFrom;
    public Transform TargetTo;

    private LineRenderer line;
    private Vector3 posFrom;
    private Vector3 posTo;
    private Vector3[] positions;
    // Start is called before the first frame update
    void Start()
    {
        line = this.GetComponent<LineRenderer>();
        if (StringPath.Length > 2)
        {
            line.positionCount = StringPath.Length;
            positions = new Vector3[StringPath.Length];
        }
        updatePositions();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        updatePositions();
        if (StringPath.Length <= 2)
        {
            if (TargetFrom != null && TargetTo != null)
            {
                line.SetPosition(0, posFrom);
                line.SetPosition(1, posTo);
            }
        }
        else
        {
            for (int i = 0; i < StringPath.Length; i++)
            {
                line.SetPosition(i,positions[i]);
            }
        }
    }

    void updatePositions()
    {
        if (StringPath.Length <= 2)
        {
            if (TargetFrom != null && TargetTo != null)
            {
                posFrom = TargetFrom.position;
                posTo = TargetTo.position;
            }
        }
        else
        {
            for(int i = 0; i < StringPath.Length; i++)
            {
                positions[i] = StringPath[i].position;
            }
        }
    }
}
