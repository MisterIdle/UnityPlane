// CODE PAR ALEXY HOUBLOUP

using UnityEngine;

public class Config : MonoBehaviour
{
    [Header("General Math Values")]
    [SerializeField] private float degreesInCircle = 360f;
    [SerializeField] private float turbulenceNormalizationOffset = 1f;
    [SerializeField] private float turbulenceNormalizationDivisor = 2f;
    [SerializeField] private float minClamp = -1f;
    [SerializeField] private float maxClamp = 1f;

    [Header("Separation & Repulsion")]
    [SerializeField] private float minDistance = 0.001f;
    [SerializeField] private float leaderRepulsion = 1.5f;

    [Header("Infinity Loop")]
    [SerializeField] private float infinityHeightMultiplier = 0.5f;
    [SerializeField] private float infinityFrequency = 2f;

    [Header("AI Speed & Clamps")]
    [SerializeField] private float aiSpeedMinDistance = 1.5f;
    [SerializeField] private float aiAlignmentThreshold = 0.2f;
    [SerializeField] private float aiBrakeFactor = 0.85f;
    [SerializeField] private float aiMinClamp = 0.82f;
    [SerializeField] private float aiMaxClamp = 1.22f;

    [Header("Shake")]
    [SerializeField] private float shakePerlinOffset = 0.5f;
    [SerializeField] private float shakeAmplitudeMult = 2f;
    [SerializeField] private float shakeTimeMultX = 2f;
    [SerializeField] private float shakeTimeMultY = 1.5f;

    private void Update()
    {
        MathI.degreesInCircle = degreesInCircle;
        MathI.turbulenceOffset = turbulenceNormalizationOffset;
        MathI.turbulenceDivisor = turbulenceNormalizationDivisor;
        MathI.minClamp = minClamp;
        MathI.maxClamp = maxClamp;

        MathI.minDistance = minDistance;
        MathI.leaderRepulsion = leaderRepulsion;

        MathI.infinityHeight = infinityHeightMultiplier;
        MathI.infinityFrequency = infinityFrequency;

        MathI.aiSpeedMinDistance = aiSpeedMinDistance;
        MathI.aiAlignmentThreshold = aiAlignmentThreshold;
        MathI.aiBrakeFactor = aiBrakeFactor;
        MathI.aiMinClamp = aiMinClamp;
        MathI.aiMaxClamp = aiMaxClamp;

        MathI.shakePerlinOffset = shakePerlinOffset;
        MathI.shakeAmplitudeMult = shakeAmplitudeMult;
        MathI.shakeTimeMultX = shakeTimeMultX;
        MathI.shakeTimeMultY = shakeTimeMultY;
    }
}
