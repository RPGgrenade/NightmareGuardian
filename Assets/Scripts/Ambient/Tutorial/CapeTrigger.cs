using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapeTrigger : MonoBehaviour
{
    public bool isActive = true;
    public float DeactivationTime = 0.48f;
    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(isActive);
    }

    private void Update()
    {
        if (SettingsHandler.Instance.TutorialDone)
            deactivate();
        if (!isActive)
        {
            DeactivationTime -= Time.deltaTime;
            if (DeactivationTime <= 0)
                this.gameObject.SetActive(isActive);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            turnOff();
        }
    }

    private void turnOff()
    {
        deactivate();
        TP_Animator.Instance.anim.SetTrigger("GetCape");
        TP_Camera.Instance.ResetToFront();
    }

    private void deactivate()
    {
        isActive = false;
        this.GetComponent<Collider>().enabled = false;
    }
}
