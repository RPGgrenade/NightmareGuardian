using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TP_Supers : MonoBehaviour
{
    public static TP_Supers Instance;
    
    [Header("Super move enablers")]
    [Tooltip("Indicator showing that one can super jump if they press the jump button in this moment in time")]
    public bool CanSuperJump = false;
    [Tooltip("Indicator showing that one can super string if they press the jump button in this moment in time")]
    public bool CanSuperString = false;
    [Tooltip("Indicator showing that one can super block if they press the jump button in this moment in time")]
    public bool CanChargeBlock = false;
    [Tooltip("Indicator showing that one can super parry if they press the jump button in this moment in time")]
    public bool CanChargeParry = false;
    [Tooltip("Indicator showing that one can super cover if they press the jump button in this moment in time")]
    public bool CanChargeCover = false;

    [Header("Super move indicators")]
    [Tooltip("Indicator showing that super jump has been activated")]
    public bool SuperJump = false;
    [Tooltip("Indicator showing that super string has been activated")]
    public bool SuperString = false;
    [Tooltip("Indicator showing that super block has been activated")]
    public bool ChargeBlock = false;
    [Tooltip("Indicator showing that super parry has been activated")]
    public bool ChargeStab = false;
    [Tooltip("Indicator showing that super cover has been activated")]
    public bool ChargeCover = false;

    public SphereCollider ShieldSphere;
    public BoxCollider CapeBox;
    public NeedleDamage NeedleStun;

    public float shieldSize = 1f;
    public float capeSize = 1f;
    public Windbox CapeWind;
    
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
    }

    // Update is called once per frame
    public void SupersUpdate()
    {
        // Add in buffering
        SuperJump = CanSuperJump && Input.GetButtonDown("Jump");
        SuperString = CanSuperString && !TP_Controller.CharacterController.isGrounded && Input.GetButtonDown("Crouch");
        ChargeStab = CanChargeParry && Input.GetButton("Needle");
        ChargeBlock = CanChargeBlock && Input.GetButton("Light");
        ChargeCover = CanChargeCover && Input.GetButton("Blanket");

        if (ChargeStab)
        {
            if (NeedleStun.StabStunMultipler < 2f)
                NeedleStun.StabStunMultipler += Time.deltaTime;
            if (TP_Motor.Instance.SprintTimer > 0f)
                TP_Motor.Instance.AddForwardMomentum(0.2f);
            else
            {
                if (TP_Controller.CharacterController.isGrounded)
                    TP_Motor.Instance.AddForwardMomentum(0.4f);
                else
                    TP_Motor.Instance.AddForwardMomentum(0.3f);
            }
        }
        if (ChargeBlock)
            shieldSize += 0.02f;
        if (ChargeCover)
        {
            capeSize += 0.02f;
            CapeWind.IncreaseLaunchMultiplier();
        }
    }

    private void LateUpdate()
    {
        ShieldSphere.radius *= shieldSize;
        CapeBox.size *= capeSize;
        ReduceHitboxes();
    }

    public void ResetStabStun()
    {
        NeedleStun.ResetStunMultiplier();
    }

    public void ReduceHitboxes()
    {
        if(!ChargeBlock)
            shieldSize = Mathf.Clamp(shieldSize - 0.04f, 1f, 1.5f);
        if(!ChargeCover)
            capeSize = Mathf.Clamp(capeSize - 0.04f, 1f, 1.5f);
    }

    public void ResetWindBox()
    {
        CapeWind.ResetLaunchMultiplier();
    }
}
