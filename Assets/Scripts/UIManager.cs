// CODE PAR ALEXY HOUBLOUP

using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player playerPlane;

    [Header("Debug")]
    [SerializeField] private TMP_Text debugTextPlayer;
    [SerializeField] private TMP_Text debugTextIA;

    private float lastAltitude;
    private bool areGizmosEnabled;

    private void Start()
    {
        lastAltitude = playerPlane.transform.position.y;
    }

    private void Update()
    {
        debugTextPlayer.enabled = areGizmosEnabled;
        debugTextIA.enabled = areGizmosEnabled;

        if (areGizmosEnabled)
            UpdateDebugText();

        areGizmosEnabled = false;
    }

    private void OnDrawGizmos()
    {
        areGizmosEnabled = true;
    }

    private void UpdateDebugText()
    {
        debugTextPlayer.text = GetPlayerFlightData(Time.deltaTime);
        debugTextIA.text = GetAIPlane();
    }

    private string GetPlayerFlightData(float pDeltaTime)
    {
        float lPitchSin = Vector3.Dot(playerPlane.transform.forward, playerPlane.GravityCenter);
        float lPitchAngle = Mathf.Asin(lPitchSin) * Mathf.Rad2Deg;
        float lRollAngle = MathI.CalculateRoll(playerPlane.transform.up, playerPlane.GravityCenter, playerPlane.transform.forward);

        float lCurrentSpeed = playerPlane.CurrentSpeed;
        float lCurrentAltitude = playerPlane.transform.position.y;

        float lClimbRate = (lCurrentAltitude - lastAltitude) / pDeltaTime;
        float lShake = MathI.CalculateTurbulence(playerPlane.transform.up, playerPlane.GravityCenter);

        lastAltitude = lCurrentAltitude;

        return $"<b>PLAYER FLIGHT DATA</b>\n\n" +
               $"<b>Speed:</b> {lCurrentSpeed:F1} m/s\n" +
               $"<b>Altitude:</b> {lCurrentAltitude:F1} m\n" +
               $"<b>Climb Rate:</b> {lClimbRate:F1} m/s\n\n" +
               $"<b>Pitch:</b> {lPitchAngle:F1}°\n" +
               $"<b>Roll:</b> {lRollAngle:F1}°\n" +
               $"<b>Shake:</b> {lShake:F2}\n\n";
    }

    private string GetAIPlane()
    {
        string lText = $"<b>AI PLANE</b>\n\n";
        int lAiCount = IAPlane.PlaneIAList.Count;

        if (lAiCount > 0)
        {
            lText += $"<b>Total Count :</b> {lAiCount}\n\n";

            foreach (IAPlane lAiPlane in IAPlane.PlaneIAList)
            {
                float lDistanceToPlayer = Vector3.Distance(playerPlane.transform.position, lAiPlane.transform.position);
                float lAiSpeed = lAiPlane.CurrentSpeed;
                float lAiAltitude = lAiPlane.transform.position.y;

                lText += $"<b>[AI {lAiPlane.planeIAid}]</b> D: {lDistanceToPlayer:F0}m | S: {lAiSpeed:F0}m/s | A: {lAiAltitude:F0}m\n";
            }
        }
        else
            lText += "<b>Press A or P to spawn</b>";

        return lText;
    }
}