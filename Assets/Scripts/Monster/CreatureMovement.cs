
using UnityEngine;

public class CreatureMovement : MonoBehaviour
{

    public static CreatureMovement Instance;
    public static CharacterController MonsterController;

    [Tooltip("How fast the Monster will move")]
    public float MotionSpeed = 10f;
    [Tooltip("How fast the monster will turn around")]
    public float RotationSpeed = 10f;
    public bool RotatingRight = true;
    public int RotationDirectionChance = 30;
    

    // Use this for initialization
    void Awake()
    {
        Instance = this;
        MonsterController = GetComponent("CharacterController") as CharacterController;
    }

    // Update is called once per frame
    void Update()
    {
        updateRotationDirection();
    }

    public void Move() //poor thing can't strafe
    {
        Vector3 force = this.transform.forward * MotionSpeed * Time.deltaTime;
        MonsterController.Move(-force); //careful with this.
    }

    public void Rotate()
    {
        if(RotatingRight)
            transform.Rotate(Vector3.up, RotationSpeed * Time.deltaTime);
        else
            transform.Rotate(Vector3.up, -RotationSpeed * Time.deltaTime);
    }

    private void updateRotationDirection()
    {
        if (Random.Range(0, RotationDirectionChance) == 1)
            RotatingRight = !RotatingRight;
    }
}
