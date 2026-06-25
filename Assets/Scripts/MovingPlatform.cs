using UnityEngine;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] public Vector3 movementDirection = Vector3.right;
    [SerializeField] public float speed = 3f;
    [SerializeField] public float distance = 5f;
    [SerializeField] public float startDelay = 0f;

    private Vector3 startPosition;
    private Vector3 previousPosition;
    private float timer;
    private HashSet<CharacterController> activeControllers = new HashSet<CharacterController>();

    private void Start()
    {
        startPosition = transform.position;
        previousPosition = startPosition;
        timer = -startDelay;

        // Check if a trigger collider already exists
        BoxCollider[] colliders = GetComponents<BoxCollider>();
        bool hasTrigger = false;
        foreach (var c in colliders)
        {
            if (c.isTrigger)
            {
                hasTrigger = true;
                break;
            }
        }

        // Automatically create a trigger collider if none exists
        if (!hasTrigger)
        {
            BoxCollider trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(1.02f, 2.0f, 1.02f);
            trigger.center = new Vector3(0f, 0.6f, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (timer < 0)
        {
            timer += Time.fixedDeltaTime;
            return;
        }

        // Ping-pong movement
        float pingPong = Mathf.PingPong(Time.time * speed, distance);
        Vector3 targetPos = startPosition + movementDirection.normalized * pingPong;
        transform.position = targetPos;
    }

    private void LateUpdate()
    {
        Vector3 platformDelta = transform.position - previousPosition;
        previousPosition = transform.position;

        if (platformDelta.sqrMagnitude > 0.0001f)
        {
            // Move any player standing on the platform
            foreach (var controller in activeControllers)
            {
                if (controller != null && controller.enabled)
                {
                    controller.Move(platformDelta);
                }
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
