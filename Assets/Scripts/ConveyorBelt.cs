using UnityEngine;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    [Header("Conveyor Belt Settings")]
    [SerializeField] public Vector3 pushDirection = Vector3.back; // relative to the belt's transform
    [SerializeField] public float pushSpeed = 4f;

    private HashSet<CharacterController> activeControllers = new HashSet<CharacterController>();

    private void Update()
    {
        // Calculate push vector in world space based on local direction
        Vector3 push = transform.TransformDirection(pushDirection).normalized * pushSpeed * Time.deltaTime;
        
        // Remove any null entries
        activeControllers.RemoveWhere(cc => cc == null);

        foreach (var controller in activeControllers)
        {
            if (controller.enabled)
            {
                controller.Move(push);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                activeControllers.Add(cc);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var cc = other.GetComponent<CharacterController>();
            if (cc != null)
            {
                activeControllers.Remove(cc);
            }
        }
    }
}
