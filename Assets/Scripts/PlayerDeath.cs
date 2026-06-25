using UnityEngine;
using System;

public class PlayerDeath : MonoBehaviour
{
    public string deadlyTag = "Enemy";
    public Transform spawnPoint;

    private CharacterController controller;

    // Static event that TrollTrigger can subscribe to
    public static event Action<GameObject> OnPlayerRespawned;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(deadlyTag))
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        controller.enabled = false;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        controller.enabled = true;

        // Notify all listeners that player has respawned
        OnPlayerRespawned?.Invoke(gameObject);
    }
}