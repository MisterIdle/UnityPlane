// CODE PAR ALEXY HOUBLOUP

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Player : Plane
{
    private const float MIN_VIGNETTE_RANGE = 0f;
    private const float MAX_VIGNETTE_RANGE = 1f;

    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private float cameraSensitivity = 75f;
    [SerializeField] private float cameraSmoothSpeed = 4.5f;
    [SerializeField] private float maxPitchAngle = 80f;
    [SerializeField] private float cameraBaseRoll = 0f;
    [SerializeField] private float cameraTurbulenceMultiplier = 0.4f;
    [SerializeField] private Color cameraGizmoColor = Color.cyan;

    [Header("Post-Processing")]
    [SerializeField] private Volume globalVolume;
    [SerializeField, Range(MIN_VIGNETTE_RANGE, MAX_VIGNETTE_RANGE)] private float maxVignetteIntensity = 0.65f;
    [SerializeField] private float vignetteSmoothSpeed = 4f;

    private float targetYaw;
    private float targetPitch;
    private float targetVignette;

    private Vector3 cameraMove;
    private Vignette vignette;

    protected override void Start()
    {
        base.Start();

        InitVignette();
    }

    private void InitVignette()
    {
        globalVolume.profile.TryGet(out vignette);
        vignette.active = true;
    }

    protected override void Update()
    {
        base.Update();

        UpdateVignette();
        UpdateCamera();
    }

    protected override void ApplyTurbulence()
    {
        base.ApplyTurbulence();

        cameraPivot.Rotate(currentTurbulenceRot * cameraTurbulenceMultiplier, Space.Self);
    }

    private void UpdateCamera()
    {
        UpdateCameraAngles();

        float lStep = cameraSmoothSpeed * Time.deltaTime;
        Quaternion lTargetRotation = Quaternion.Euler(targetPitch, targetYaw, cameraBaseRoll);

        cameraPivot.localRotation = Quaternion.Slerp(cameraPivot.localRotation, lTargetRotation, lStep);
    }

    private void UpdateCameraAngles()
    {
        float lSensitivity = cameraSensitivity * Time.deltaTime;

        targetYaw += cameraMove.x * lSensitivity;
        targetPitch -= cameraMove.y * lSensitivity;
        targetPitch = Mathf.Clamp(targetPitch, -maxPitchAngle, maxPitchAngle);
    }

    private void UpdateVignette()
    {
        float lInversion = MathI.CalculateTurbulence(transform.up, gravityCenter);
        float lStep = vignetteSmoothSpeed * Time.deltaTime;

        targetVignette = lInversion * maxVignetteIntensity;
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetVignette, lStep);
    }

    public override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        DrawRayGizmo(cameraPivot.position, cameraPivot.forward, cameraGizmoColor);
    }

    public void OnRoll(InputAction.CallbackContext pContext)
    {
        rollMove = pContext.ReadValue<Vector2>();
    }

    public void OnCamera(InputAction.CallbackContext pContext)
    {
        cameraMove = pContext.ReadValue<Vector2>();
    }
}
