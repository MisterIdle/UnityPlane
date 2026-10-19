// CODE PAR ALEXY HOUBLOUP

using UnityEngine;
using UnityEngine.InputSystem;

public enum FormationType
{
    BackV,
    BackLine,
    VerticalCircle,
    VerticalOrbit,
    HorizontalCircle,
    HorizontalOrbit,
    SideWall,
    InfinityLoop
}

public class Formation : MonoBehaviour
{
    [Header("Formation Selection")]
    [SerializeField] private FormationType currentFormation = FormationType.BackV;
    [SerializeField] private float leaderDistanceZ = 30f;
    [SerializeField] private float leaderSafeBubble = 22f;

    [Header("V, Line & Wall Spacing")]
    [SerializeField] private float spacingX = 18f;
    [SerializeField] private float spacingZ = 20f;

    [Header("Circles, Orbits")]
    [SerializeField] private float circleRadius = 32f;
    [SerializeField] private float circleOffsetZ = 18f;
    [SerializeField] private float circleOffsetY = 0f;
    [SerializeField] private float orbitSpinSpeed = 16f;
    [SerializeField] private float staticCircleAngle = 0f;

    [SerializeField, Range(0, 360)] private float offsetY = 0;
    [SerializeField, Range(0, 360)] private float offsetZ = 0;

    [Header("Gizmos")]
    [SerializeField] private Color safeBubbleGizmoColor = new Color(0f, 1f, 0.5f, 0.25f);

    private float currentAngle;

    public float LeaderSafeBubble => leaderSafeBubble;

    private void Update()
    {
        UpdateSpinAngle();
        ReadFormationInput();
    }

    private void UpdateSpinAngle()
    {
        currentAngle += orbitSpinSpeed * Time.deltaTime;
    }

    private void ReadFormationInput()
    {
        Keyboard lKeyboard = Keyboard.current;

        if (lKeyboard.digit1Key.wasPressedThisFrame) 
            currentFormation = FormationType.BackV;
        
        else if (lKeyboard.digit2Key.wasPressedThisFrame) 
            currentFormation = FormationType.SideWall;
        
        else if (lKeyboard.digit3Key.wasPressedThisFrame) 
            currentFormation = FormationType.VerticalCircle;
        
        else if (lKeyboard.digit4Key.wasPressedThisFrame) 
            currentFormation = FormationType.VerticalOrbit;
        
        else if (lKeyboard.digit5Key.wasPressedThisFrame) 
            currentFormation = FormationType.HorizontalCircle;
        
        else if (lKeyboard.digit6Key.wasPressedThisFrame) 
            currentFormation = FormationType.HorizontalOrbit;
        
        else if (lKeyboard.digit7Key.wasPressedThisFrame) 
            currentFormation = FormationType.BackLine;
        
        else if (lKeyboard.digit8Key.wasPressedThisFrame) 
            currentFormation = FormationType.InfinityLoop;
    }

    public Vector3 GetSlotLocalOffset(int pIndex, int pPlaneIA)
    {
        switch (currentFormation)
        {
            case FormationType.BackV:
                return MathI.SetVShape(pIndex, leaderDistanceZ, spacingX, spacingZ);

            case FormationType.SideWall:
                return MathI.SetSideWall(pIndex, leaderDistanceZ, spacingX);
            
            case FormationType.BackLine:
                return MathI.SetLine(pIndex, leaderDistanceZ, spacingZ);

            case FormationType.VerticalCircle:
                return MathI.SetVerticalCircle(pIndex, pPlaneIA, circleRadius, circleOffsetZ, staticCircleAngle);

            case FormationType.HorizontalCircle:
                return MathI.SetHorizontalCircle(pIndex, pPlaneIA, circleRadius, circleOffsetY, staticCircleAngle);

            case FormationType.VerticalOrbit:
                return MathI.SetVerticalCircle(pIndex, pPlaneIA, circleRadius, circleOffsetZ, currentAngle);

            case FormationType.HorizontalOrbit:
                return MathI.SetHorizontalCircle(pIndex, pPlaneIA, circleRadius, circleOffsetY, currentAngle);

            case FormationType.InfinityLoop:
                return MathI.SetInfinityLoop(pIndex, pPlaneIA, circleRadius, circleOffsetZ, currentAngle);

            default:
                return Vector3.zero;
        }
    }

    public Vector3 GetSlotWorldPosition(Vector3 pLocalSlot) => MathI.LocalToWorld(transform, pLocalSlot);

    private void OnDrawGizmos()
    {
        Gizmos.color = safeBubbleGizmoColor;
        Gizmos.DrawWireSphere(transform.position, leaderSafeBubble);
    }
}