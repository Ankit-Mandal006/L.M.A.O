using UnityEngine;

/// <summary>
/// A smooth third-person follow camera that tracks the player with a stable fixed perspective.
/// </summary>
public class PlayerFollowCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // The player transform

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0f, 5f, -8f);
    public float followSmoothTime = 0.15f;
    public bool useFixedRotation = true; // Stable fixed perspective

    [Header("Look Settings (when useFixedRotation is false)")]
    public Vector3 lookOffset = new Vector3(0f, 1f, 0f);
    public float rotationSmoothSpeed = 8f;
    public float lookAheadDistance = 3f;

    [Header("Collision")]
    public float minDistance = 1.5f;
    public LayerMask collisionLayers = 1;

    [Header("Boundaries")]
    public float minYPosition = -8f;

    private Vector3 currentVelocity;
    private Quaternion initialRotation;

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (target != null)
        {
            // Position camera at target + offset initially
            transform.position = target.position + offset;
            
            // Calculate initial rotation to look at the player
            Vector3 lookTarget = target.position + lookOffset;
            Vector3 lookDirection = lookTarget - transform.position;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                initialRotation = Quaternion.LookRotation(lookDirection);
            }
            else
            {
                initialRotation = transform.rotation;
            }
            
            if (!useFixedRotation)
            {
                transform.rotation = initialRotation;
            }
            else
            {
                transform.rotation = initialRotation;
            }
        }
        else
        {
            initialRotation = transform.rotation;
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Calculate desired position based on player position + offset
        Vector3 desiredPosition = target.position + offset;

        if (desiredPosition.y < minYPosition)
        {
            desiredPosition.y = minYPosition;
        }

        // Camera collision check to prevent clipping
        Vector3 targetHeadPos = target.position + lookOffset;
        Vector3 collisionDirection = desiredPosition - targetHeadPos;
        float targetDistance = collisionDirection.magnitude;

        if (targetDistance > 0.001f)
        {
            collisionDirection.Normalize();
            RaycastHit hit;
            float sphereRadius = 0.4f;

            if (Physics.SphereCast(targetHeadPos, sphereRadius, collisionDirection, out hit, targetDistance, collisionLayers))
            {
                float safeDistance = Mathf.Max(minDistance, hit.distance - 0.2f);
                desiredPosition = targetHeadPos + collisionDirection * safeDistance;
            }
        }

        // Smooth position follow
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            followSmoothTime
        );

        // Rotation follow
        if (useFixedRotation)
        {
            // Maintain stable initial rotation
            transform.rotation = initialRotation;
        }
        else
        {
            // Smooth look at target with look-ahead
            Vector3 lookTarget = target.position + lookOffset;
            if (lookAheadDistance > 0f)
            {
                lookTarget += target.forward * lookAheadDistance;
            }

            Vector3 lookDirection = lookTarget - transform.position;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSmoothSpeed * Time.deltaTime
                );
            }
        }
    }

    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = target.position + offset;
        currentVelocity = Vector3.zero;
        if (!useFixedRotation)
        {
            transform.LookAt(target.position + lookOffset);
        }
    }
}
