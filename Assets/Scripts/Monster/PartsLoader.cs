using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartsLoader : MonoBehaviour
{
    [Tooltip("The objects to load after enabling the Game Object to minimize strain")]
    public List<GameObject> PartsToSlowLoad = new List<GameObject>();

    private void OnEnable()
    {
        SetPartsEnabled();
    }

    private void OnDisable()
    {
        SetPartsDisabled();
    }

    IEnumerator setPartsEnabled()
    {
        foreach (GameObject part in PartsToSlowLoad)
        {
            if (!part.activeSelf)
            {
                part.SetActive(true);
                yield return null;
            }
        }
    }

    public void SetPartEnabled(GameObject bodyPart)
    {
        foreach (GameObject part in PartsToSlowLoad)
        {
            if (part == bodyPart)
            {
                part.SetActive(true);
                return;
            }
        }
    }

    public void SetPartDisabled(GameObject bodyPart)
    {
        foreach (GameObject part in PartsToSlowLoad)
        {
            if (part == bodyPart)
            {
                part.SetActive(false);
                return;
            }
        }
    }

    public void SetPartsEnabled()
    {
        StartCoroutine(setPartsEnabled());
    }

    public void SetPartsDisabled()
    {
        foreach (GameObject part in PartsToSlowLoad)
        {
            part.SetActive(false);
        }
    }
}
