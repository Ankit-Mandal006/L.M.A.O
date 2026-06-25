using UnityEngine;

public class RotatingHammer : MonoBehaviour
{
    [Header("Rotation Settings")]
    [SerializeField] public float rotationSpeed = 90f; // degrees per second
    [SerializeField] public Vector3 rotationAxis = Vector3.up;

    private void Update()
    {
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
    }
}
