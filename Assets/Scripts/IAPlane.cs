// CODE PAR ALEXY HOUBLOUP

using UnityEngine;
using System.Collections.Generic;

public class IAPlane : Plane
{
    private const int START_INDEX = 0;

    [Header("AI References")]
    [SerializeField] private Player leader;
    [SerializeField] private Formation formation;

    [Header("AI Separation")]
    [SerializeField] private float separationDistance = 18f;
    [SerializeField] private float separationStrength = 25f;

    [Header("AI Smoothness & Banking")]
    [SerializeField] private float formationTransitionSpeed = 0.7f;
    [SerializeField] private float speedSmoothness = 1.2f;
    [SerializeField] private float steeringSensitivity = 0.8f;
    [SerializeField] private float turnSmoothness = 1.6f;
    [SerializeField] private float catchUp = 0.35f;
    [SerializeField] private float maxBankAngle = 70f;
    [SerializeField] private float bankResponsiveness = 3.5f;

    [Header("AI Thresholds & Clamps")]
    [SerializeField] private float minDirectionDistance = 0.01f;
    [SerializeField] private float alignmentBlendDistance = 30f;
    [SerializeField] private float minSteeringClamp = -1f;
    [SerializeField] private float maxSteeringClamp = 1f;
    [SerializeField] private float rollAngle = 45f;
    [SerializeField] private float leaderWeight = 0.5f;

    [Header("AI Debug Gizmos")]
    [SerializeField] private float slotGizmoRadius = 1.5f;
    [SerializeField] private Color slotGizmoColor = Color.cyan;
    [SerializeField] private Color targetLineGizmoColor = Color.yellow;
    [SerializeField] private Color separationRayGizmoColor = Color.magenta;

    public static List<IAPlane> PlaneIAList = new List<IAPlane>();
    public int planeIAid;

    private bool slotInit;

    private Vector3 smoothedLocalSlot;
    private Vector3 debugSlotPos;
    private Vector3 debugSeparationVec;
    private Vector3 finalTargetPos;

    private void Awake()
    {
        leader = FindFirstObjectByType<Player>();
        formation = FindFirstObjectByType<Formation>();
    }

    private void OnEnable()
    {
        PlaneIAList.Add(this);
        UpdateAllIndices();
    }

    private void OnDisable()
    {
        PlaneIAList.Remove(this);
        UpdateAllIndices();
    }

    private static void UpdateAllIndices()
    {
        for (int lIndex = START_INDEX; lIndex < PlaneIAList.Count; lIndex++)
            PlaneIAList[lIndex].planeIAid = lIndex;
    }

    protected override void Update()
    {
        UpdateAIBehavior();
        base.Update();
    }

    private void UpdateAIBehavior()
    {
        UpdateSlotPosition();
        UpdateSeparation();
        
        UpdateSpeed();
        
        UpdateSteering();
    }

    private void UpdateSlotPosition()
    {
        Vector3 lTargetLocalSlot = formation.GetSlotLocalOffset(planeIAid, PlaneIAList.Count);

        if (!slotInit)
        {
            smoothedLocalSlot = lTargetLocalSlot;
            slotInit = true;
        }
        else
            smoothedLocalSlot = Vector3.Lerp(smoothedLocalSlot, lTargetLocalSlot, formationTransitionSpeed * Time.deltaTime);

        debugSlotPos = formation.GetSlotWorldPosition(smoothedLocalSlot);
    }

    private void UpdateSeparation()
    {
        debugSeparationVec = MathI.CalculateSeparation(transform, PlaneIAList, leader.transform, separationDistance, formation.LeaderSafeBubble, separationStrength);
        finalTargetPos = debugSlotPos + debugSeparationVec;
    }

    private void UpdateSpeed()
    {
        float lTargetSpeed = MathI.CalculateAISpeed(transform.position, transform.forward, finalTargetPos, leader.transform.forward, leader.CurrentSpeed, catchUp);
        float lSmoothStep = speedSmoothness * Time.deltaTime;

        speed = Mathf.Lerp(speed, lTargetSpeed, lSmoothStep);
    }

    private void UpdateSteering()
    {
        Vector3 lForward = Forward();
        Vector3 lLocalDir = transform.InverseTransformDirection(lForward);

        float lTurnIntensity = lLocalDir.x * bankResponsiveness;

        ApplySteering(lLocalDir, lTurnIntensity);
        ApplyRotation(lForward, lTurnIntensity);
    }

    private Vector3 Forward()
    {
        Vector3 lToTarget = finalTargetPos - transform.position;
        Vector3 lDirToSlot = leader.transform.forward;

        float lDistance = lToTarget.magnitude;
        float lRatio = Mathf.Clamp01(lDistance / alignmentBlendDistance);

        if (lDistance > minDirectionDistance)
            lDirToSlot = lToTarget.normalized;

        return Vector3.Lerp(leader.transform.forward, lDirToSlot, lRatio).normalized;
    }

    private void ApplySteering(Vector3 pLocalDir, float pTurnIntensity)
    {
        float lPitchCommand = Mathf.Clamp(pLocalDir.y * steeringSensitivity, minSteeringClamp, maxSteeringClamp);
        float lVirtualPitch = MathI.GetPitchInput(lPitchCommand, invertPitch);

        float lLeaderRollError = MathI.CalculateRoll(transform.up, leader.transform.up, transform.forward) / rollAngle;
        float lRawRoll = pTurnIntensity - (lLeaderRollError * leaderWeight);
        float lClampedRoll = Mathf.Clamp(lRawRoll, minSteeringClamp, maxSteeringClamp);
        float lVirtualRoll = MathI.GetHorizontal(lClampedRoll, invertHorizontal);

        float lClampedYaw = Mathf.Clamp(pLocalDir.x * steeringSensitivity, minSteeringClamp, maxSteeringClamp);

        rollMove = new Vector2(lVirtualRoll, lVirtualPitch);
        yawInput = MathI.GetHorizontal(lClampedYaw, invertHorizontal);
    }

    private void ApplyRotation(Vector3 pDesiredForward, float pTurnIntensity)
    {
        float lSmoothStep = turnSmoothness * Time.deltaTime;
        
        Vector3 lBankedUp = MathI.CalculateBankedUp(pDesiredForward, leader.transform.up, pTurnIntensity, maxBankAngle);
        Quaternion lTargetRotation = Quaternion.LookRotation(pDesiredForward, lBankedUp);

        transform.rotation = Quaternion.Slerp(transform.rotation, lTargetRotation, lSmoothStep);
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        Gizmos.color = slotGizmoColor;
        Gizmos.DrawWireSphere(debugSlotPos, slotGizmoRadius);

        Gizmos.color = targetLineGizmoColor;
        Gizmos.DrawLine(transform.position, finalTargetPos);

        Gizmos.color = separationRayGizmoColor;
        Gizmos.DrawRay(transform.position, debugSeparationVec);
    }
}
