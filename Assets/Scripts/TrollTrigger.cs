using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrollTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("The collider that acts as the trigger zone")]
    public BoxCollider triggerZone;
    
    [Tooltip("Tag to detect (usually 'Player')")]
    public string targetTag = "Player";
    
    [Tooltip("Can this trigger be activated multiple times?")]
    public bool reusable = false;
    
    [Tooltip("Delay before movement starts after trigger")]
    public float activationDelay = 0f;

    [Header("Movement Settings")]
    [Tooltip("The object that will move (can be this object or a different one)")]
    public Transform movingObject;
    
    [Tooltip("Movement type")]
    public MovementType movementType = MovementType.MoveToTarget;
    
    [Tooltip("Movement speed")]
    public float moveSpeed = 5f;
    
    [Tooltip("Movement curve for smooth motion")]
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Path Points")]
    [Tooltip("List of waypoints for the object to follow")]
    public List<Transform> pathPoints = new List<Transform>();
    
    [Tooltip("Should the object loop through waypoints?")]
    public bool loopPath = false;
    
    [Tooltip("Should the object return to start position?")]
    public bool returnToStart = false;

    [Header("Player Collision Settings")]
    [Tooltip("Disable player controller when hit by moving objects")]
    public bool disablePlayerOnCollision = false;
    
    [Tooltip("Game objects with colliders that should disable player on collision")]
    public List<GameObject> collisionObjects = new List<GameObject>();
    
    [Tooltip("Name of the third person controller script to disable")]
    public string controllerScriptName = "ThirdPersonController";

    [Header("Visual Settings")]
    [Tooltip("Color of the trigger zone gizmo")]
    public Color triggerColor = new Color(1f, 0f, 0f, 0.3f);
    
    [Tooltip("Color of the path gizmo")]
    public Color pathColor = Color.yellow;
    
    [Tooltip("Show trigger zone in game")]
    public bool showTriggerInGame = false;

    [Header("Audio (Optional)")]
    public AudioClip activationSound;
    public AudioClip movementSound;
    private AudioSource audioSource;

    // Private variables
    private bool hasTriggered = false;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Coroutine movementCoroutine;
    private bool isMoving = false;
    private MonoBehaviour playerController;
    private List<CollisionDetector> collisionDetectors = new List<CollisionDetector>();

    public enum MovementType
    {
        MoveToTarget,
        FollowPath,
        PingPong,
        RotateInPlace,
        ScaleChange
    }

    private void Start()
    {
        // Set up the trigger zone
        if (triggerZone == null)
        {
            triggerZone = GetComponent<BoxCollider>();
            if (triggerZone == null)
            {
                triggerZone = gameObject.AddComponent<BoxCollider>();
            }
        }
        triggerZone.isTrigger = true;

        // Set moving object to self if not assigned
        if (movingObject == null)
        {
            movingObject = transform;
        }

        // Store start position and rotation
        startPosition = movingObject.position;
        startRotation = movingObject.rotation;

        // Set up audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (activationSound != null || movementSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Hide trigger renderer in game if needed
        if (!showTriggerInGame)
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        // Set up collision detection on specified objects
        if (disablePlayerOnCollision)
        {
            SetupCollisionDetectors();
        }
    }

    private void SetupCollisionDetectors()
    {
        // If no objects specified, use the moving object
        if (collisionObjects.Count == 0 && movingObject != null)
        {
            collisionObjects.Add(movingObject.gameObject);
        }

        // Add collision detector to each object
        foreach (GameObject obj in collisionObjects)
        {
            if (obj != null)
            {
                CollisionDetector detector = obj.GetComponent<CollisionDetector>();
                if (detector == null)
                {
                    detector = obj.AddComponent<CollisionDetector>();
                }
                detector.Initialize(this, targetTag);
                collisionDetectors.Add(detector);
            }
        }
    }

    public void OnPlayerCollision(GameObject player)
    {
        if (!disablePlayerOnCollision || !isMoving) return;

        DisablePlayerController(player);
    }

    private void DisablePlayerController(GameObject player)
    {
        if (playerController == null)
        {
            // Try to find the controller script
            Component[] components = player.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp.GetType().Name == controllerScriptName)
                {
                    playerController = comp as MonoBehaviour;
                    break;
                }
            }
        }

        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("Player controller disabled due to collision with moving object");
        }
        else
        {
            Debug.LogWarning("Could not find " + controllerScriptName + " script on player");
        }
    }

    private void EnablePlayerController()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
            Debug.Log("Player controller re-enabled");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && !reusable) return;
        
        if (other.CompareTag(targetTag))
        {
            if (activationDelay > 0)
            {
                StartCoroutine(DelayedActivation());
            }
            else
            {
                Activate();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag) && returnToStart)
        {
            // Stop current movement
            if (movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine);
            }
            
            isMoving = false;
            
            // Return to start position
            movementCoroutine = StartCoroutine(MoveToTarget(startPosition));
            
            // Reset trigger if reusable
            if (reusable)
            {
                hasTriggered = false;
            }
        }
    }

    private IEnumerator DelayedActivation()
    {
        yield return new WaitForSeconds(activationDelay);
        Activate();
    }

    private void Activate()
    {
        hasTriggered = true;
        isMoving = true;
        
        // Play activation sound
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }

        // Start movement based on type
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        switch (movementType)
        {
            case MovementType.MoveToTarget:
                if (pathPoints.Count > 0)
                {
                    movementCoroutine = StartCoroutine(MoveToTarget(pathPoints[0].position));
                }
                break;
                
            case MovementType.FollowPath:
                if (pathPoints.Count > 0)
                {
                    movementCoroutine = StartCoroutine(FollowPath());
                }
                break;
                
            case MovementType.PingPong:
                if (pathPoints.Count > 0)
                {
                    movementCoroutine = StartCoroutine(PingPongMovement());
                }
                break;
                
            case MovementType.RotateInPlace:
                if (pathPoints.Count > 0)
                {
                    movementCoroutine = StartCoroutine(RotateToTarget(pathPoints[0].rotation));
                }
                break;
                
            case MovementType.ScaleChange:
                if (pathPoints.Count > 0)
                {
                    movementCoroutine = StartCoroutine(ChangeScale(pathPoints[0].localScale));
                }
                break;
        }

        // Play movement sound
        if (audioSource != null && movementSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = movementSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private IEnumerator MoveToTarget(Vector3 targetPosition)
    {
        float elapsed = 0f;
        float distance = Vector3.Distance(movingObject.position, targetPosition);
        float duration = distance / moveSpeed;

        Vector3 startPos = movingObject.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float curveValue = movementCurve.Evaluate(t);
            
            movingObject.position = Vector3.Lerp(startPos, targetPosition, curveValue);
            yield return null;
        }

        movingObject.position = targetPosition;
        StopMovementSound();
    }

    private IEnumerator FollowPath()
    {
        int currentPoint = 0;
        
        do
        {
            for (int i = currentPoint; i < pathPoints.Count; i++)
            {
                if (pathPoints[i] != null)
                {
                    yield return StartCoroutine(MoveToTarget(pathPoints[i].position));
                    yield return new WaitForSeconds(0.1f);
                }
            }
            currentPoint = 0;
        } while (loopPath);

        isMoving = false;
        StopMovementSound();
        EnablePlayerController();
    }

    private IEnumerator PingPongMovement()
    {
        if (pathPoints.Count < 1) yield break;

        Vector3 pointA = startPosition;
        Vector3 pointB = pathPoints[0].position;

        while (true)
        {
            yield return StartCoroutine(MoveToTarget(pointB));
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(MoveToTarget(pointA));
            yield return new WaitForSeconds(0.5f);
            
            if (!reusable) break;
        }

        isMoving = false;
        StopMovementSound();
        EnablePlayerController();
    }

    private IEnumerator RotateToTarget(Quaternion targetRotation)
    {
        float elapsed = 0f;
        float duration = 1f;
        Quaternion startRot = movingObject.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = movementCurve.Evaluate(elapsed / duration);
            movingObject.rotation = Quaternion.Lerp(startRot, targetRotation, t);
            yield return null;
        }

        movingObject.rotation = targetRotation;
        isMoving = false;
        StopMovementSound();
        EnablePlayerController();
    }

    private IEnumerator ChangeScale(Vector3 targetScale)
    {
        float elapsed = 0f;
        float duration = 1f;
        Vector3 startScale = movingObject.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = movementCurve.Evaluate(elapsed / duration);
            movingObject.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        movingObject.localScale = targetScale;
        isMoving = false;
        StopMovementSound();
        EnablePlayerController();
    }

    private void StopMovementSound()
    {
        if (audioSource != null && audioSource.isPlaying && audioSource.clip == movementSound)
        {
            audioSource.Stop();
            audioSource.loop = false;
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
        isMoving = false;
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        movingObject.position = startPosition;
        movingObject.rotation = startRotation;
        EnablePlayerController();
    }

    private void OnDrawGizmos()
    {
        // Draw trigger zone
        if (triggerZone != null)
        {
            Gizmos.color = triggerColor;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(triggerZone.center, triggerZone.size);
            Gizmos.matrix = oldMatrix;
        }

        // Draw path
        if (pathPoints.Count > 0)
        {
            Gizmos.color = pathColor;
            
            Vector3 startPos = (movingObject != null) ? movingObject.position : transform.position;
            
            // Draw line from object to first point
            if (pathPoints[0] != null)
            {
                Gizmos.DrawLine(startPos, pathPoints[0].position);
                Gizmos.DrawWireSphere(pathPoints[0].position, 0.3f);
            }

            // Draw lines between waypoints
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                if (pathPoints[i] != null && pathPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(pathPoints[i].position, pathPoints[i + 1].position);
                    Gizmos.DrawWireSphere(pathPoints[i + 1].position, 0.3f);
                }
            }

            // Draw loop connection if enabled
            if (loopPath && pathPoints.Count > 1 && pathPoints[0] != null && pathPoints[pathPoints.Count - 1] != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(pathPoints[pathPoints.Count - 1].position, pathPoints[0].position);
            }
        }
    }

    // Helper class for collision detection
    private class CollisionDetector : MonoBehaviour
    {
        private TrollTrigger parentTrigger;
        private string playerTag;

        public void Initialize(TrollTrigger trigger, string tag)
        {
            parentTrigger = trigger;
            playerTag = tag;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(playerTag))
            {
                parentTrigger.OnPlayerCollision(collision.gameObject);
            }
        }
    }
}