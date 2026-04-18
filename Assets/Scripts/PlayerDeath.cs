using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public string deadlyTag = "Enemy";
    public Transform spawnPoint;

    private CharacterController controller;

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

    void Respawn()
    {
        controller.enabled = false;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        controller.enabled = true;
    }
}