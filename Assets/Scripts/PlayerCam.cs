using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [Header("Mouse Sensitivity")]
    public float sensX = 400f;
    public float sensY = 400f;

    [Header("References")]
    public Transform orientation;
    public Transform cameraHolder; // The transform that holds the camera (head/eye position)

    private float xRotation = 0f;
    private float yRotation = 0f;

    private void Start()
    {
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        // Get raw mouse input
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensY;

        // Accumulate rotations
        yRotation += mouseX;
        xRotation -= mouseY;

        // Clamp vertical rotation (can't look too far up/down)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply rotations
        // Camera/Head rotates up/down
        if (cameraHolder != null)
        {
            cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // Orientation/Body rotates left/right
        if (orientation != null)
        {
            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

    // Allow unlocking cursor for pause menu or UI
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
