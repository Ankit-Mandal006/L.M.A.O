using UnityEngine;
using Unity.Cinemachine;

public class RoomCameraFollow : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public CinemachineSplineDolly dolly;
    public Transform cameraTransform;

    [Header("Room Bounds")]
    public Transform roomStart;
    public Transform roomEnd;

    [Header("Spline Range")]
    [Range(0f, 1f)] public float splineStart = 0f;
    [Range(0f, 1f)] public float splineEnd = 1f;

    [Header("Follow")]
    public float followSmoothTime = 0.25f;
    public float offsetBehindPlayer = 0.15f; // How far behind on the spline (0-1 range offset)

    [Header("Distance Repulsion")]
    public float preferredDistance = 7f;
    public float repelStrength = 0.08f;
    public float repelSmoothness = 3f;

    private float velocity;
    private float repelOffset;

    void LateUpdate()
    {
        if (player == null || dolly == null)
            return;

        // ===== ROOM PROGRESSION =====

        Vector3 roomDirection = roomEnd.position - roomStart.position;
        Vector3 playerDirection = player.position - roomStart.position;

        float roomLength = roomDirection.magnitude;

        if (roomLength <= 0.01f)
            return;

        Vector3 normalizedRoomDir = roomDirection.normalized;

        float projectedDistance = Vector3.Dot(
            playerDirection,
            normalizedRoomDir
        );

        float progress = Mathf.Clamp01(projectedDistance / roomLength);

        // Map to spline range
        float playerSplinePos = Mathf.Lerp(
            splineStart,
            splineEnd,
            progress
        );

        // ===== CAMERA BEHIND PLAYER =====
        // Subtract offset so camera is behind the player on the spline
        float targetSplinePos = playerSplinePos - offsetBehindPlayer;

        // Clamp to spline range
        targetSplinePos = Mathf.Clamp(targetSplinePos, splineStart, splineEnd);

        // ===== DISTANCE CHECK =====

        float currentDistance = Vector3.Distance(
            cameraTransform.position,
            player.position
        );

        // Repel target
        float targetRepel = 0f;

        if (currentDistance < preferredDistance)
        {
            float closeness = 1f - (currentDistance / preferredDistance);
            targetRepel = closeness * repelStrength;
        }

        // Smooth repel offset
        repelOffset = Mathf.Lerp(
            repelOffset,
            targetRepel,
            repelSmoothness * Time.deltaTime
        );

        // Final target
        float finalTarget = targetSplinePos - repelOffset;

        // Clamp final target to valid spline range
        finalTarget = Mathf.Clamp(finalTarget, splineStart, splineEnd);

        // Smooth camera movement
        dolly.CameraPosition = Mathf.SmoothDamp(
            dolly.CameraPosition,
            finalTarget,
            ref velocity,
            followSmoothTime
        );
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        if (roomStart == null || roomEnd == null || player == null)
            return;

        // Draw room bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(roomStart.position, roomEnd.position);
        Gizmos.DrawWireSphere(roomStart.position, 0.5f);
        Gizmos.DrawWireSphere(roomEnd.position, 0.5f);

        // Draw player position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, 0.3f);

        if (Application.isPlaying && cameraTransform != null)
        {
            // Draw camera position
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(cameraTransform.position, 0.35f);
            
            // Draw connection
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(cameraTransform.position, player.position);
        }
    }
}