// CODE PAR ALEXY HOUBLOUP

using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneSpawner : MonoBehaviour
{
    [SerializeField] private GameObject planeIAPrefab;
    [SerializeField] private Transform spawnPoint;

    private void Update()
    {
        ReadInput();
    }

    private void ReadInput()
    {
        Keyboard lKeyboard = Keyboard.current;
        Gamepad lGamepad = Gamepad.current;

        if (lGamepad is null)
            return;

        bool lSpawnPressed = lKeyboard.pKey.wasPressedThisFrame || lKeyboard.numpadPlusKey.wasPressedThisFrame || lGamepad.aButton.wasPressedThisFrame;

        if (lSpawnPressed)
            SpawnPlane();
    }

    private void SpawnPlane()
    {
        Instantiate(planeIAPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
