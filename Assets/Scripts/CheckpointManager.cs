using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("The actual Spawn Point GameObject that your respawn logic references.")]
    [SerializeField] private Transform spawnPoint;

    private Checkpoint currentCheckpoint;

    public void UpdateCheckpoint(Checkpoint newCheckpoint)
    {
        // 1. Deactivate the old checkpoint if one exists
        if (currentCheckpoint != null)
        {
            currentCheckpoint.SetActive(false);
        }

        // 2. Set and activate the new checkpoint
        currentCheckpoint = newCheckpoint;
        currentCheckpoint.SetActive(true);

        // 3. Move the spawn point to the new checkpoint's position
        if (spawnPoint != null)
        {
            spawnPoint.position = currentCheckpoint.transform.position;
            // Optional: Match rotation if your player respawns facing a specific way
            spawnPoint.rotation = currentCheckpoint.transform.rotation; 
            
            Debug.Log($"Checkpoint updated! Spawn point moved to: {spawnPoint.position}");
        }
        else
        {
            Debug.LogWarning("Spawn Point Transform is not assigned in the CheckpointManager!");
        }
    }
}