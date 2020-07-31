using UnityEngine;

public class BugBehavior : MonoBehaviour {

    [Header("Bug Properties")]
    [Tooltip("How long the bug has lived")]
    public float Life = 0f;
    [Tooltip("How long the bug lives in seconds")]
    public float LifeSpan = 500f;
    [Tooltip("How fast the bug with move")]
    public float Speed = 4f;
    [Tooltip("How much the bug will likely move vertically")]
    public float VerticalBias = 1f;
    [Tooltip("How random the bug will move (smaller number is more random)")]
    public int Craziness = 30;
    public int Attention = 200;
    public GameObject CurrentTarget;
    [Tooltip("Stuff the bug likes moving around")]
    public string[] NiceThings;

    private Animator anim;
    private Rigidbody body;
    private System.Random rng;
    private float verticalDirection = 5;

	// Use this for initialization
	void Start () {
        anim = this.GetComponent<Animator>();
        rng = new System.Random();
        body = this.GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        anim.SetFloat("Speed",body.velocity.magnitude);
        Life += Time.deltaTime;
        if (Life > LifeSpan)
            GameObject.Destroy(this.gameObject);
        updateAngle();
        updatePosition();
	}

    private void updatePosition()
    {
        Vector3 movementDirection = this.transform.forward;
        movementDirection += new Vector3(0,verticalDirection,0);
        body.velocity = (movementDirection * Speed);
    }

    private void updateAngle()
    {
        int change = rng.Next(0,Craziness);
        if(change == 1)
        {
            changeAngles();
        }
    }

    private void changeAngles()
    {
        verticalDirection = Random.Range(-VerticalBias, VerticalBias);
        this.transform.rotation = Random.rotation;
        transform.eulerAngles = new Vector3(0,transform.eulerAngles.y,0);
    }

    private void angleTowards(Transform target)
    {
        transform.LookAt(target, this.transform.up);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (CurrentTarget == null)
        {
            foreach (string thing in NiceThings)
            {
                bool reactionChance = (Random.Range(0, 100) == 1);
                if (collider.tag == thing)
                {
                    if (reactionChance)
                    {
                        changeAngles();
                        break;
                    }
                    else
                    {
                        CurrentTarget = collider.gameObject;
                        break;
                    }
                }
            }
        }
        else
        {
            angleTowards(CurrentTarget.transform);
            bool attentionChance = (Random.Range(0, Attention) == 1);
            if (attentionChance)
                CurrentTarget = null;
            return;
        }
        changeAngles();
    }

    private void OnTriggerStay(Collider collider)
    {
        if (CurrentTarget == null)
        {
            foreach (string thing in NiceThings)
            {
                bool reactionChance = (Random.Range(0, 100) == 1);
                if (collider.tag == thing)
                {
                    if (reactionChance)
                    {
                        changeAngles();
                        break;
                    }
                    else
                    {
                        CurrentTarget = collider.gameObject;
                        break;
                    }
                }
            }
        }
        else
        {
            angleTowards(CurrentTarget.transform);
            bool attentionChance = (Random.Range(0, Attention) == 1);
            if (attentionChance)
                CurrentTarget = null;
            return;
        }
    }
}
