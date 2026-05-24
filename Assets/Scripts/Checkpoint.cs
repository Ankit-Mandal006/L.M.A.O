using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private string playerTag = "Player";
    
    [Header("Animations")]
    [SerializeField] private Animator animator;
    [SerializeField] private string activeTriggerName = "Activate";
    [SerializeField] private string inactiveTriggerName = "Deactivate";

    private bool isActive = false;
    private CheckpointManager manager;

    private void Start()
    {
        // Find the manager in the scene
        manager = FindFirstObjectByType<CheckpointManager>();
        if (manager == null)
        {
            Debug.LogError("Checkpoint Manager missing from the scene!", this);
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player touched it and it isn't already active
        if (other.CompareTag(playerTag) && !isActive)
        {
            manager.UpdateCheckpoint(this);
        }
    }

    public void SetActive(bool status)
    {
        isActive = status;

        if (animator != null)
        {
            // Fire the corresponding animator triggers
            string triggerToPlay = isActive ? activeTriggerName : inactiveTriggerName;
            animator.SetTrigger(triggerToPlay);
        }
    }
}