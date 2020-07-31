using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faucet : MonoBehaviour
{
    public Transform DropletPoint;
    public GameObject Droplet;
    public bool Open = false;
    public float DropTime = 2f;
    [Tooltip("How long until the faucest closes")]
    public float CloseTime = 40f;

    private Animator anim;
    private float dropTimer = 0f;
    private float closeTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        if (anim == null)
            anim = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        dropTimer += Time.deltaTime;
        if (dropTimer >= DropTime && Open)
        {
            dropTimer = 0f;
            drop();
        }
        if (Open)
        {
            closeTimer += Time.deltaTime;
            if (closeTimer >= CloseTime)
            {
                Open = false;
                closeTimer = 0f;
            }
        }

        anim.SetBool("Open", Open);
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.transform.tag == "Monster" && other.gameObject.layer == LayerMask.NameToLayer("Monster"))
            Open = true;
        if (other.transform.tag == "Player" && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            Open = false;
    }

    private void drop()
    {
        GameObject.Instantiate(Droplet, DropletPoint.position, Quaternion.identity);
    }
}
