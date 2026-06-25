using UnityEngine;
using System.Collections.Generic;

public class TiltingPlatform : MonoBehaviour
{
    [Header("Tilting Settings")]
    [SerializeField] public float maxTiltAngle = 15f;
    [SerializeField] public float tiltSpeed = 2f;
    [SerializeField] public float returnSpeed = 1f;
    [SerializeField] public float sensitivity = 1.5f; // how fast it tilts based on player distance from pivot

    private Quaternion neutralRotation;
    private List<Transform> activePlayers = new List<Transform>();

    private void Start()
    {
        neutralRotation = transform.rotation;
    }

    private void Update()
    {
        // Clean up any destroyed/inactive references
        activePlayers.RemoveAll(t => t == null);

        if (activePlayers.Count > 0)
        {
            // Calculate average offset in local space of the platform
            Vector3 averageLocalPos = Vector3.zero;
            foreach (var player in activePlayers)
            {
                averageLocalPos += transform.InverseTransformPoint(player.position);
            }
            averageLocalPos /= activePlayers.Count;

            // Tilt platform based on the offset
            // Rotate around local X axis based on local Z position,
            // and rotate around local Z axis based on local X position.
            float targetTiltAngleX = -averageLocalPos.z * sensitivity;
            float targetTiltAngleZ = averageLocalPos.x * sensitivity;

            // Clamp the angles
            targetTiltAngleX = Mathf.Clamp(targetTiltAngleX, -maxTiltAngle, maxTiltAngle);
            targetTiltAngleZ = Mathf.Clamp(targetTiltAngleZ, -maxTiltAngle, maxTiltAngle);

            Quaternion targetRotation = neutralRotation * Quaternion.Euler(targetTiltAngleX, 0f, targetTiltAngleZ);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltSpeed * Time.deltaTime);
        }
        else
        {
            // Return to neutral position
            transform.rotation = Quaternion.Slerp(transform.rotation, neutralRotation, returnSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!activePlayers.Contains(other.transform))
            {
                activePlayers.Add(other.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            activePlayers.Remove(other.transform);
        }
    }
}
