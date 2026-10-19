// CODE PAR ALEXY HOUBLOUP

using System.Collections.Generic;
using UnityEngine;

public static class MathI
{
    private const int DIVISOR = 2;
    private const int RANK = 1;
    private const int MIN_TOTAL_COUNT = 1;

    private const float POSITIVE_SIDE = 1f;
    private const float NEGATIVE_SIDE = -1f;

    public static float degreesInCircle = 360f;
    public static float turbulenceOffset = 1f;
    public static float turbulenceDivisor = 2f;
    public static float minClamp = -1f;
    public static float maxClamp = 1f;

    public static float minDistance = 0.001f;
    public static float leaderRepulsion = 1.5f;

    public static float infinityHeight = 0.5f;
    public static float infinityFrequency = 2f;

    public static float aiSpeedMinDistance = 1.5f;
    public static float aiAlignmentThreshold = 0.2f;
    public static float aiBrakeFactor = 0.85f;
    public static float aiMinClamp = 0.82f;
    public static float aiMaxClamp = 1.22f;

    public static float shakePerlinOffset = 0.5f;
    public static float shakeAmplitudeMult = 2f;
    public static float shakeTimeMultX = 2f;
    public static float shakeTimeMultY = 1.5f;

    public static float GetPitchInput(float pPitch, bool pInvertPitch)
    {
        if (pInvertPitch)
            return pPitch;

        return -pPitch;
    }

    public static float GetHorizontal(float pHorizontal, bool pInvertHorizontal)
    {
        if (pInvertHorizontal)
            return -pHorizontal;

        return pHorizontal;
    }

    public static float Autopilot(Vector3 pForward, Vector3 pGravityCenter, float pThreshold, float pBaseSpeed)
    {
        // Alternative: 
        // float lVerticalAngle = Vector3.Angle(pForward, pGravityCenter);
        // float lVerticalDot = Mathf.Cos(lVerticalAngle * Mathf.Deg2Rad);

        float lVerticalDot = Vector3.Dot(pForward, pGravityCenter);

        if (Mathf.Abs(lVerticalDot) > pThreshold)
            return 0;

        return pBaseSpeed;
    }

    public static float CalculateTurbulence(Vector3 pUp, Vector3 pGravityCenter)
    {
        // Alternative:
        // float lAngle = Vector3.Angle(pUp, pGravityCenter);
        // return Mathf.InverseLerp(0f, 180f, lAngle);

        float lOrientationDot = Vector3.Dot(pUp, pGravityCenter);

        return (turbulenceOffset - lOrientationDot) / turbulenceDivisor;
    }

    public static Vector3 CalculateRepulsion(Vector3 pSelfPos, Vector3 pOtherPos, float pMinDist, float pStrength)
    {
        Vector3 lDiff = pSelfPos - pOtherPos;
        float lDist = Vector3.Distance(pSelfPos, pOtherPos);

        if (lDist >= pMinDist || lDist <= minDistance)
            return Vector3.zero;

        float lFactor = (pMinDist - lDist) / pMinDist;

        return lDiff.normalized * (lFactor * pStrength);
    }

    public static Vector3 CalculateSeparation(Transform pSelf, List<IAPlane> pPlaneAI, Transform pLeader, float pMinPlaneAIDist, float pLeaderBubble, float pStrength)
    {
        Vector3 lSeparation = Vector3.zero;

        foreach (IAPlane lOther in pPlaneAI)
        {
            if (lOther.transform == pSelf)
                continue;

            lSeparation += CalculateRepulsion(pSelf.position, lOther.transform.position, pMinPlaneAIDist, pStrength);
        }

        float lLeaderStrength = pStrength * leaderRepulsion;
        lSeparation += CalculateRepulsion(pSelf.position, pLeader.position, pLeaderBubble, lLeaderStrength);

        return lSeparation;
    }

    public static float CalculateRoll(Vector3 pUp, Vector3 pGravityCenter, Vector3 pForward)
    {
        // Alternative: float lAbsoluteAngle = Vector3.Angle(pUp, pGravityCenter);

        float lDot = Vector3.Dot(pUp, pGravityCenter);
        float lClampedDot = Mathf.Clamp(lDot, minClamp, maxClamp);
        float lAbsoluteAngle = Mathf.Acos(lClampedDot) * Mathf.Rad2Deg;

        // Alternative: return Vector3.SignedAngle(pUp, pGravityCenter, pForward);

        Vector3 lCross = Vector3.Cross(pUp, pGravityCenter);
        float lDeterminant = Vector3.Dot(lCross, pForward);
        float lSign = Mathf.Sign(lDeterminant);

        return lAbsoluteAngle * lSign;
    }

    public static Vector2 Rotate2D(Vector2 pVector, float pAngleDeg)
    {
        Vector3 lRotated = Quaternion.Euler(0f, 0f, pAngleDeg) * new Vector3(pVector.x, pVector.y, 0f);
        return new Vector2(lRotated.x, lRotated.y);
    }

    public static Vector3 LocalToWorld(Transform pOrigin, Vector3 pLocalOffset)
    {
        // Alternative: return pOrigin.TransformPoint(pLocalOffset);

        Matrix4x4 lMatrix = pOrigin.localToWorldMatrix;

        return lMatrix.MultiplyPoint3x4(pLocalOffset);
    }

    public static int GetRank(int pIndex) => (pIndex / DIVISOR) + RANK;

    public static float GetSide(int pIndex)
    {
        if (pIndex % DIVISOR == 0)
            return NEGATIVE_SIDE;

        return POSITIVE_SIDE;
    }

    public static Vector3 SetVShape(int pIndex, float pOffsetZ, float pSpacingX, float pSpacingZ)
    {
        int lRank = GetRank(pIndex);
        float lSide = GetSide(pIndex);

        float lPosX = lSide * lRank * pSpacingX;
        float lPosZ = -pOffsetZ - (lRank * pSpacingZ);

        return new Vector3(lPosX, 0, lPosZ);
    }

    public static Vector3 SetLine(int pIndex, float pOffsetZ, float pSpacingZ)
    {
        float lPosZ = -pOffsetZ - ((pIndex + RANK) * pSpacingZ);

        return new Vector3(0, 0, lPosZ);
    }

    public static Vector3 SetSideWall(int pIndex, float pOffsetZ, float pSpacingX)
    {
        int lRank = GetRank(pIndex);
        float lSide = GetSide(pIndex);

        float lPosX = lSide * lRank * pSpacingX;

        return new Vector3(lPosX, 0, -pOffsetZ);
    }

    public static Vector2 SetCirclePoint(int pIndex, int pTotal, float pRadius, float pAngleDeg)
    {
        float lBaseAngle = degreesInCircle / Mathf.Max(MIN_TOTAL_COUNT, pTotal) * pIndex;
        float lRad = lBaseAngle * Mathf.Deg2Rad;

        float lPosX = pRadius * Mathf.Cos(lRad);
        float lPosY = pRadius * Mathf.Sin(lRad);

        Vector2 lCircle = new Vector2(lPosX, lPosY);

        return Rotate2D(lCircle, pAngleDeg);
    }

    public static Vector3 SetVerticalCircle(int pIndex, int pTotal, float pRadius, float pOffsetZ, float pAngleDeg)
    {
        Vector2 lRotated = SetCirclePoint(pIndex, pTotal, pRadius, pAngleDeg);

        return new Vector3(lRotated.x, lRotated.y, -pOffsetZ);
    }

    public static Vector3 SetHorizontalCircle(int pIndex, int pTotal, float pRadius, float pHeightY, float pAngleDeg)
    {
        Vector2 lRotated = SetCirclePoint(pIndex, pTotal, pRadius, pAngleDeg);

        return new Vector3(lRotated.x, pHeightY, lRotated.y);
    }

    public static Vector3 SetInfinityLoop(int pIndex, int pTotal, float pRadius, float pOffsetZ, float pSpinAngleDeg)
    {
        float lBaseAngle = degreesInCircle / Mathf.Max(MIN_TOTAL_COUNT, pTotal) * pIndex;
        float lRad = (lBaseAngle + pSpinAngleDeg) * Mathf.Deg2Rad;

        float lPosX = pRadius * Mathf.Cos(lRad);
        float lPosY = pRadius * infinityHeight * Mathf.Sin(infinityFrequency * lRad);

        return new Vector3(lPosX, lPosY, -pOffsetZ);
    }

    public static Vector3 CalculateBankedUp(Vector3 pForward, Vector3 pReferenceUp, float pTurnAmount, float pMaxBankAngle)
    {
        float lBankAngle = Mathf.Clamp(-pTurnAmount, minClamp, maxClamp) * pMaxBankAngle;
        Quaternion lBankRotation = Quaternion.AngleAxis(lBankAngle, pForward);

        return lBankRotation * pReferenceUp;
    }

    public static float CalculateAISpeed(Vector3 pSelfPos, Vector3 pSelfForward, Vector3 pTargetPos, Vector3 pLeaderForward, float pLeaderSpeed, float pCatchUpForce)
    {
        Vector3 lToTarget = pTargetPos - pSelfPos;
        float lDistance = lToTarget.magnitude;

        if (lDistance < aiSpeedMinDistance)
            return pLeaderSpeed;

        float lForward = Vector3.Dot(pLeaderForward, lToTarget);
        float lAlignment = Vector3.Dot(pSelfForward, lToTarget.normalized);

        if (lAlignment < aiAlignmentThreshold)
            return pLeaderSpeed * aiBrakeFactor;

        float lTargetSpeed = pLeaderSpeed + (lForward * pCatchUpForce * lAlignment);

        float lMinSpeed = pLeaderSpeed * aiMinClamp;
        float lMaxSpeed = pLeaderSpeed * aiMaxClamp;

        return Mathf.Clamp(lTargetSpeed, lMinSpeed, lMaxSpeed);
    }

    public static Vector3 Shake(float pTime, Vector3 pMultiplier)
    {
        float lNoiseX = (Mathf.PerlinNoise(pTime, 0) - shakePerlinOffset) * shakeAmplitudeMult * Mathf.Sin(pTime * shakeTimeMultX);
        float lNoiseY = (Mathf.PerlinNoise(0, pTime) - shakePerlinOffset) * shakeAmplitudeMult * Mathf.Cos(pTime * shakeTimeMultY);
        float lNoiseZ = (Mathf.PerlinNoise(pTime, pTime) - shakePerlinOffset) * shakeAmplitudeMult * Mathf.Sin(pTime);

        float lFinalX = lNoiseX * pMultiplier.x;
        float lFinalY = lNoiseY * pMultiplier.y;
        float lFinalZ = lNoiseZ * pMultiplier.z;

        return new Vector3(lFinalX, lFinalY, lFinalZ);
    }
}