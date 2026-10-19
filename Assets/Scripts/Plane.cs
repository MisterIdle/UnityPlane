// CODE PAR ALEXY HOUBLOUP

using UnityEngine;

public class Plane : MonoBehaviour
{
    private const float MIN_THRESHOLD_RANGE = 0f;
    private const float MAX_THRESHOLD_RANGE = 1f;

    [Header("Movement")]
    [SerializeField] protected float speed = 55f;
    [SerializeField] private float rollSpeed = 40f;
    [SerializeField] private float pitchSpeed = 22f;
    [SerializeField] private float yawSpeed = 14f;
    [SerializeField] private float autopilotSpeed = 0.8f;
    [SerializeField, Range(MIN_THRESHOLD_RANGE, MAX_THRESHOLD_RANGE)] private float verticalThreshold = 0.95f;
    [SerializeField] protected bool invertPitch = true;
    [SerializeField] protected bool invertHorizontal = true;

    [Header("Turbulence & Wind")]
    [SerializeField] private float baseTurbulenceIntensity = 4f;
    [SerializeField] private float baseTurbulenceSpeed = 2.5f;

    [SerializeField, Range(MIN_THRESHOLD_RANGE, MAX_THRESHOLD_RANGE)] private float turbulenceThreshold = 0.05f;
    [SerializeField] private float maxTurbulenceIntensity = 18f;
    [SerializeField] private float turbulenceSpeed = 5f;
    [SerializeField] private float minRandomTurbulenceTime = 0f;
    [SerializeField] private float maxRandomTurbulenceTime = 100f;
    [SerializeField] private Vector3 turbulenceNoise = new Vector3(2f, 2f, 3f);

    [Header("Gravity")]
    [SerializeField] private Transform gravityPoint;

    [Header("Gamefeel & Animations")]
    [SerializeField] private GameObject model;
    [SerializeField] private float bankingMultiplier = 0.85f;
    [SerializeField] private float inputSmoothSpeed = 2.5f;
    [SerializeField] private float spriteBaseYaw = 0f;

    [Header("Propeller")]
    [SerializeField] private Transform bladesTransform;
    [SerializeField] private float bladesBaseSpeed = 3200f;
    [SerializeField] private float bladesPitchModifier = 100f;
    [SerializeField] private float bladesSmoothness = 3.5f;

    [Header("Debug & Gizmos")]
    [SerializeField] private float gizmoLineLength = 8f;
    [SerializeField] private float gravityGizmoRadius = 0.5f;
    [SerializeField] private Color gravityGizmoColor = Color.yellow;
    [SerializeField] private Color forwardGizmoColor = Color.blue;
    [SerializeField] private Color upGizmoColor = Color.green;
    [SerializeField] private Color rightGizmoColor = Color.red;

    protected float yawInput;
    protected float smoothedYaw;
    protected float turbulenceTime;
    protected float currentBladesSpeed;

    protected Vector3 rollMove;
    protected Vector3 smoothedRollMove;
    protected Vector3 currentTurbulenceRot;
    protected Vector3 gravityCenter = Vector3.up;

    public Vector3 GravityCenter => gravityCenter;

    public float CurrentSpeed => speed;

    protected virtual void Start()
    {
        currentBladesSpeed = bladesBaseSpeed;
        turbulenceTime = Random.Range(minRandomTurbulenceTime, maxRandomTurbulenceTime);
    }

    protected virtual void Update()
    {
        UpdateGravity();

        UpdateMovement();
        UpdateRotation();

        AnimateBlades();

        ApplyTurbulence();
    }

    protected virtual void UpdateRotation()
    {
        Smooth();

        float lPitch = CalculatePitchAngle();
        float lRoll = CalculateRollAngle();
        float lYaw = CalculateYawAngle();

        ApplyWorldRotation(lPitch, lRoll, lYaw);
        UpdateSpriteBanking();
    }

    private void Smooth()
    {
        float lStep = inputSmoothSpeed * Time.deltaTime;

        smoothedRollMove = Vector3.Lerp(smoothedRollMove, rollMove, lStep);
        smoothedYaw = Mathf.Lerp(smoothedYaw, yawInput, lStep);
    }

    private float CalculatePitchAngle()
    {
        float lPitchInput = MathI.GetPitchInput(smoothedRollMove.y, invertPitch);

        return lPitchInput * pitchSpeed * Time.deltaTime;
    }

    private float CalculateYawAngle()
    {
        float lYawInput = MathI.GetHorizontal(smoothedYaw, invertHorizontal);

        return lYawInput * yawSpeed * Time.deltaTime;
    }

    private float CalculateRollAngle()
    {
        float lRollInput = MathI.GetHorizontal(smoothedRollMove.x, invertHorizontal);
        float lAutopilotSpeed = MathI.Autopilot(transform.forward, gravityCenter, verticalThreshold, autopilotSpeed);
        float lRoll = MathI.CalculateRoll(transform.up, gravityCenter, transform.forward);

        return ((lRollInput * rollSpeed) + (lRoll * lAutopilotSpeed)) * Time.deltaTime;
    }


    private void ApplyWorldRotation(float pPitch, float pRoll, float pYaw)
    {
        transform.Rotate(transform.right, pPitch, Space.World);
        transform.Rotate(transform.forward, pRoll, Space.World);
        transform.Rotate(transform.up, pYaw, Space.World);
    }

    private void UpdateSpriteBanking()
    {
        float lPitchInput = MathI.GetPitchInput(smoothedRollMove.y, invertPitch);
        float lRollInput = MathI.GetHorizontal(smoothedRollMove.x, invertHorizontal);

        float lTargetPitch = lPitchInput * pitchSpeed * bankingMultiplier;
        float lTargetRoll = lRollInput * rollSpeed * bankingMultiplier;

        model.transform.localRotation = Quaternion.Euler(lTargetPitch, spriteBaseYaw, lTargetRoll);
    }

    protected virtual void UpdateMovement()
    {
        Vector3 lVelocity = transform.forward * (speed * Time.deltaTime);

        transform.position += lVelocity;
    }

    protected virtual void ApplyTurbulence()
    {
        float lFactor = MathI.CalculateTurbulence(transform.up, gravityCenter);
        float lIntensity = CalculateTurbulenceIntensity(lFactor);

        UpdateTurbulence(lFactor);

        Vector3 lRot = MathI.Shake(turbulenceTime, turbulenceNoise);
        currentTurbulenceRot = lRot * (lIntensity * Time.deltaTime);

        transform.Rotate(currentTurbulenceRot, Space.Self);
    }

    private void UpdateTurbulence(float pFactor)
    {
        float lSpeed = baseTurbulenceSpeed;

        if (pFactor > turbulenceThreshold)
            lSpeed += turbulenceSpeed * pFactor;

        turbulenceTime += Time.deltaTime * lSpeed;
    }

    private float CalculateTurbulenceIntensity(float pFactor)
    {
        float lIntensity = baseTurbulenceIntensity;

        if (pFactor > turbulenceThreshold)
            lIntensity += maxTurbulenceIntensity * pFactor;

        return lIntensity;
    }

    protected virtual void UpdateGravity()
    {
        gravityPoint.rotation = Quaternion.identity;
        gravityCenter = gravityPoint.up;
    }

    protected virtual void AnimateBlades()
    {
        float lVerticalDot = Vector3.Dot(transform.forward, gravityCenter);
        float lTargetSpeed = bladesBaseSpeed - (lVerticalDot * bladesPitchModifier);
        float lStep = bladesSmoothness * Time.deltaTime;

        currentBladesSpeed = Mathf.Lerp(currentBladesSpeed, lTargetSpeed, lStep);

        bladesTransform.Rotate(Vector3.forward, currentBladesSpeed * Time.deltaTime, Space.Self);
    }

    protected void DrawRayGizmo(Vector3 pOrigin, Vector3 pDirection, Color pColor)
    {
        Gizmos.color = pColor;
        Gizmos.DrawRay(pOrigin, pDirection * gizmoLineLength);
    }

    public virtual void OnDrawGizmos()
    {
        DrawRayGizmo(transform.position, transform.forward, forwardGizmoColor);
        DrawRayGizmo(transform.position, transform.up, upGizmoColor);
        DrawRayGizmo(transform.position, transform.right, rightGizmoColor);
        DrawRayGizmo(gravityPoint.position, gravityPoint.up, gravityGizmoColor);

        Gizmos.DrawWireSphere(gravityPoint.position, gravityGizmoRadius);
    }
}