using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    public Transform cameraOffsetPoint; // Point on player where camera should follow (e.g., head position)

    [Header("Settings")]
    public Vector3 offsetPosition = new Vector3(0, 0.6f, 0); // Offset from player center (adjust for eye height)
    public float smoothSpeed = 0.1f; // Set to 1 for instant follow

    private void LateUpdate()
    {
        if (playerTransform == null)
            return;

        // Calculate target position
        Vector3 targetPosition = playerTransform.position + offsetPosition;

        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed);

        // Make camera look forward
        transform.forward = playerTransform.forward;
    }
}
