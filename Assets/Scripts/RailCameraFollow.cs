using UnityEngine;
using Unity.Cinemachine;

public class RailCameraFollow : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public CinemachineSplineDolly splineDolly;

    [Header("Level Bounds")]
    public float levelMinX;
    public float levelMaxX = 20f;

    [Header("Camera Settings")]
    public float followSpeed = 5f;

    void Update()
    {
        // Convert player X position into normalized 0-1 range
        float normalizedPos = Mathf.InverseLerp(
            levelMinX,
            levelMaxX,
            player.position.x
        );

        // Smoothly move camera on spline
        splineDolly.CameraPosition = Mathf.Lerp(
            splineDolly.CameraPosition,
            normalizedPos,
            followSpeed * Time.deltaTime
        );
    }
}