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

    [Header("Follow Settings")]
    public float followSmoothTime = 0.25f;
    public float behindOffset = 0.15f; // How far behind player (0-1 along room)

    [Header("Distance Repulsion")]
    public float preferredDistance = 7f;
    public float repelStrength = 0.08f;
    public float repelSmoothness = 3f;

    private float velocity;
    private float repelOffset;

    void LateUpdate()
    {
        if (player == null || dolly == null || roomStart == null || roomEnd == null)
            return;

        // ===== ROOM PROGRESSION =====

        Vector3 roomDirection = roomEnd.position - roomStart.position;
        Vector3 playerDirection = player.position - roomStart.position;

        float roomLength = roomDirection.magnitude;

        if (roomLength <= 0.01f)
            return;

        Vector3 normalizedRoomDir = roomDirection.normalized;

        // Project player position onto room axis
        float projectedDistance = Vector3.Dot(playerDirection, normalizedRoomDir);
        float progress = Mathf.Clamp01(projectedDistance / roomLength);

        // ===== CAMERA ALWAYS BEHIND PLAYER =====
        // Camera is always offset backwards from player's progress
        // If player goes back, camera naturally ends up in front (player now faces it)

        float targetProgress = progress - behindOffset;
        targetProgress = Mathf.Clamp01(targetProgress);

        float playerSplinePos = Mathf.Lerp(splineStart, splineEnd, targetProgress);

        // ===== DISTANCE REPULSION =====

        if (cameraTransform != null)
        {
            float currentDistance = Vector3.Distance(cameraTransform.position, player.position);

            float targetRepel = 0f;

            if (currentDistance < preferredDistance)
            {
                float closeness = 1f - (currentDistance / preferredDistance);
                targetRepel = closeness * repelStrength;
            }

            repelOffset = Mathf.Lerp(repelOffset, targetRepel, repelSmoothness * Time.deltaTime);
        }

        // ===== FINAL TARGET =====

        float finalTarget = playerSplinePos - repelOffset;
        finalTarget = Mathf.Clamp(finalTarget, splineStart, splineEnd);

        // Smooth camera movement
        dolly.CameraPosition = Mathf.SmoothDamp(
            dolly.CameraPosition,
            finalTarget,
            ref velocity,
            followSmoothTime
        );
    }

    void OnDrawGizmos()
    {
        if (roomStart == null || roomEnd == null || player == null)
            return;

        // Draw room bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(roomStart.position, roomEnd.position);
        Gizmos.DrawWireSphere(roomStart.position, 0.5f);
        Gizmos.DrawWireSphere(roomEnd.position, 0.5f);

        // Draw room direction arrow
        Gizmos.color = Color.red;
        Vector3 roomDir = (roomEnd.position - roomStart.position).normalized;
        Vector3 arrowPos = roomStart.position + roomDir * 2f;
        Gizmos.DrawRay(roomStart.position, roomDir * 2f);
        
        // Arrow head
        Vector3 right = Vector3.Cross(Vector3.up, roomDir).normalized * 0.3f;
        Gizmos.DrawLine(arrowPos, arrowPos - roomDir * 0.5f + right);
        Gizmos.DrawLine(arrowPos, arrowPos - roomDir * 0.5f - right);

        // Draw player position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(player.position, 0.3f);

        if (Application.isPlaying)
        {
            if (cameraTransform != null)
            {
                // Draw camera
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(cameraTransform.position, 0.35f);
                
                // Draw connection
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(cameraTransform.position, player.position);
            }

            // Draw room progression
            Vector3 roomDirection = roomEnd.position - roomStart.position;
            Vector3 playerDirection = player.position - roomStart.position;
            float roomLen = roomDirection.magnitude;
            
            if (roomLen > 0.01f)
            {
                float proj = Vector3.Dot(playerDirection, roomDirection.normalized);
                Vector3 projPoint = roomStart.position + roomDirection.normalized * proj;
                
                Gizmos.color = Color.white;
                Gizmos.DrawLine(player.position, projPoint);
                Gizmos.DrawWireSphere(projPoint, 0.2f);
            }
        }
    }
}