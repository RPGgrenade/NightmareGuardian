using UnityEngine;
using System.Collections.Generic;
using UnityEngine.PostProcessing;

public class AimChanger : MonoBehaviour
{
    public static AimChanger Instance;

    public GameObject Target;
    //public GameObject Reference;

    public Aim AimType = Aim.Default;
    [Tooltip("Offsets of the default Aiming Type")]
    [Header("Default Offsets")]
    public float HorizontalDefaultOffset = 2.4f;
    public float VerticalDefaultOffset = 0f;
    [Header("Aim Offsets")]
    public float HorizontalAimOffset = 1.4f;
    public float VerticalAimOffset = 0.3f;
    public float GrappleAimDistance = 1f;
    [Header("Monster Angle Offsets")]
    public float HorizontalMonsterOffset = 3f;
    public float VerticalMonsterOffset = 1f;
    public float MonsterAimDistance = 2f;
    public float RotationVertical = -20f;
    public float RotationForward = 10f;
    [Header("Pause Offsets")]
    public float HorizontalPauseOffset = 0f;
    public float VerticalPauseOffset = 0.1f;
    public float PauseAimDistance = 0.1f;
    public float PauseNearClip = 0.25f;
    [Header("Smoothing")]
    public float DefaultTransitionSmoothing = 1f;
    public float AimTransitionSmoothing = 1f;
    public float AerialAimTransitionSmoothing = 5f;
    public float MonsterTransitionSmoothing = 1f;
    public float PauseTransitionSmoothing = 0.2f;
    [Header("Vignette Intensity")]
    public PostProcessingProfile Profile;
    public float VignetteChangeSpeed = 0.05f;
    [Range(0f,1f)]
    public float DefaultVignetteIntensity = 0f;
    [Range(0f, 1f)]
    public float AimVignetteIntensity = 0.5f;
    [Range(0f, 1f)]
    public float MonsterVignetteIntensity = 0.3f;
    [Range(0f, 1f)]
    public float PauseVignetteIntensity = 0.1f;
    [Header("Monster Tracking")]
    public float MonsterRight = 0f;
    public float MonsterUp = 0f;
    public float MonsterVisualPrecision = 0.3f;
    public float PlayerRight = 0f;
    public float PlayerUp = 0f;
    public float PlayerVisualPrecision = 0.5f;
    public float FieldOfViewChangeSpeed = 0.5f;

    private Vector3 defaultLocalPos = Vector3.zero;
    private Vector3 newLocalPos;
    private Transform monsterTarget;

    public enum Aim
    {
        Default, Monster, Grapple, Pause
    }

    void Awake() 
    {
        Instance = this;
        newLocalPos = defaultLocalPos;
    }

    void Update() 
    {
        if(AimType == Aim.Grapple && TP_Camera.Instance.Distance == GrappleAimDistance)
        {
            TP_Motor.Instance.SnapAlignCharacterWithAim();
        }
        UpdateAim();
        UpdateVignette();
        UpdateFOV();
    }

    public void GiveMonsterTarget(Transform monster)
    {
        monsterTarget = monster;
    }

    public void RemovesMonsterTarget()
    {
        monsterTarget = null;
    }

    public void SetAim(Aim aim)
    {
        AimType = aim;
    }

    public void UpdateFOV()
    {
        if (AimType == Aim.Monster && monsterTarget != null)
        {
            Vector3 monsterPositionAtTeddyHeight = new Vector3(monsterTarget.position.x, Camera.main.transform.position.y, monsterTarget.position.z);
            float distanceBetweenCameraAndMonster = Vector3.Distance(monsterPositionAtTeddyHeight, Camera.main.transform.position);
            if (distanceBetweenCameraAndMonster > 20f)
                distanceBetweenCameraAndMonster = 20f;
            float FOVDifference = 80 - distanceBetweenCameraAndMonster;

            if (Camera.main.fieldOfView < FOVDifference)
                Camera.main.fieldOfView += Time.deltaTime * FieldOfViewChangeSpeed;
            else
                Camera.main.fieldOfView = FOVDifference;
        }
        else if((AimType != Aim.Monster || monsterTarget == null) && TP_Stats.Instance.Noise <= 0f)
        {
            if (Camera.main.fieldOfView > 60f)
                Camera.main.fieldOfView -= Time.deltaTime * FieldOfViewChangeSpeed;
            else
                Camera.main.fieldOfView = 60f;
        }
    }

    void UpdateVignette()
    {
        float targetIntensity = 0f;
        if (AimType == Aim.Default)
            targetIntensity = DefaultVignetteIntensity;
        else if (AimType == Aim.Grapple)
            targetIntensity = AimVignetteIntensity;
        else if (AimType == Aim.Monster)
            targetIntensity = MonsterVignetteIntensity;
        else if (AimType == Aim.Pause)
            targetIntensity = PauseVignetteIntensity;

        VignetteModel.Settings settings = Profile.vignette.settings;

        float differenceBetweenIntensity = Mathf.Abs(settings.intensity - targetIntensity);

        if (settings.intensity < targetIntensity)
            settings.intensity += Time.deltaTime * VignetteChangeSpeed * differenceBetweenIntensity;
        else if (settings.intensity > targetIntensity)
            settings.intensity -= Time.deltaTime * VignetteChangeSpeed * differenceBetweenIntensity;

        if(settings.intensity < 0)
            settings.intensity = 0;
        else if (settings.intensity > 1)
            settings.intensity = 1;

        Profile.vignette.settings = settings;
    }

    public void UpdateAim() 
    {
        bool isTargetFacingRight = TP_Camera.Instance.TargetIsFacingRight;
        if (AimType == Aim.Default)
        {
            //Target.transform.localPosition = defaultLocalPos;
            if (isTargetFacingRight)
                CameraTransition(-HorizontalDefaultOffset, VerticalDefaultOffset, DefaultTransitionSmoothing);
            else
                CameraTransition(HorizontalDefaultOffset, VerticalDefaultOffset, DefaultTransitionSmoothing);
        }
        else if (AimType == Aim.Monster)
        {
            if(monsterTarget == null)
            {
                AimType = Aim.Default;
                return;
            }

            Transform cameraTrans = Camera.main.transform;
            if (monsterTarget != null)
            { 
                MonsterRight = cameraTrans.InverseTransformPoint(monsterTarget.position).x;
                MonsterUp = cameraTrans.InverseTransformPoint(monsterTarget.position).y;
                PlayerRight = cameraTrans.InverseTransformPoint(this.transform.position).x;
                PlayerUp = cameraTrans.InverseTransformPoint(this.transform.position).y;
            }
            //if (monsterTarget != null)
            //{
            //    Vector3 positionBetweenTargets = monsterTarget.transform.position - this.transform.position;
            //    MonsterRight = Camera.main.transform.InverseTransformPoint(positionBetweenTargets).x;
            //    MonsterUp = Camera.main.transform.InverseTransformPoint(positionBetweenTargets).y;
            //}
            if (!TP_Controller.Instance.paused)
            {
                //Debug.Log("Monster: " + monsterTarget.position + " Camera: " + cameraTrans.position);
                Transform monsterHeadPosition = monsterTarget.root.GetComponentInChildren<TurnToLook>().transform;
                Vector3 directionBetweenCameraAndMonster = (monsterHeadPosition.position - cameraTrans.position).normalized;
                Vector3 directionBetweenCameraAndPlayer = (this.transform.position - cameraTrans.position).normalized;

                float dotProdMonster = Vector3.Dot(directionBetweenCameraAndMonster, cameraTrans.forward);
                float dotProdPlayer = Vector3.Dot(directionBetweenCameraAndPlayer, cameraTrans.forward);

                bool isMonsterOnScreen = dotProdMonster > MonsterVisualPrecision;
                bool isPlayerOnScreen = dotProdPlayer > PlayerVisualPrecision;

                if (isMonsterOnScreen && isPlayerOnScreen)
                    CameraTransition(HorizontalMonsterOffset * -MonsterRight, VerticalMonsterOffset * -MonsterUp, MonsterTransitionSmoothing);
                else
                {
                    if(MonsterRight < 0)
                        CameraTransition(HorizontalMonsterOffset, VerticalMonsterOffset, MonsterTransitionSmoothing);
                    else if (MonsterRight > 0)
                        CameraTransition(-HorizontalMonsterOffset, VerticalMonsterOffset, MonsterTransitionSmoothing);
                }
            }
        }
        else if (AimType == Aim.Grapple)
        {
            if(TP_Controller.CharacterController.isGrounded)
                CameraTransition(HorizontalAimOffset, VerticalAimOffset, GrappleAimDistance, AimTransitionSmoothing);
            else
                CameraTransition(HorizontalAimOffset, VerticalAimOffset, GrappleAimDistance,
                    AimTransitionSmoothing * 5);
        }
        else if (AimType == Aim.Pause)
        {
            CameraTransition(HorizontalPauseOffset, VerticalPauseOffset, PauseAimDistance, PauseTransitionSmoothing);
        }
    }
    public void CameraTransition(float Horizontal, float Vertical, float Distance, float Smoothing) 
    {
        var Vector = new Vector3(Horizontal,Vertical,0) + defaultLocalPos;
        newLocalPos = Vector3.Lerp(newLocalPos, Vector, Smoothing * Time.deltaTime);
        TP_Camera.Instance.SetCameraAimDistance(Distance);
        TP_Camera.Instance.SetOffset(newLocalPos);
    }

    public void CameraTransition(float Horizontal, float Vertical, float Smoothing)
    {
        var Vector = new Vector3(Horizontal,Vertical, 0) + defaultLocalPos;
        newLocalPos = Vector3.Lerp(newLocalPos, Vector, Smoothing * Time.deltaTime);
        TP_Camera.Instance.SetOffset(newLocalPos);
        //if(AimType == Aim.Monster) //Still not working properly for this stuff. TODO
        //{
        //    Vector3 Rot = new Vector3(RotationVertical, 0, RotationForward);
        //    Quaternion newRot = TP_Camera.Instance.transform.rotation * Quaternion.Euler(Rot);
        //    Quaternion.RotateTowards(TP_Camera.Instance.transform.rotation, 
        //        newRot, MonsterTransitionSmoothing * Time.deltaTime);
        //}
    }
}