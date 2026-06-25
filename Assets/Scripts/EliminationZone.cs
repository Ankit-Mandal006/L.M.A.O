using UnityEngine;

public class EliminationZone : MonoBehaviour
{
    [Header("Elimination Settings")]
    [SerializeField] public ParticleSystem eliminationParticles;
    [SerializeField] public AudioClip eliminationSound;
    [SerializeField] [Range(0f, 1f)] public float soundVolume = 0.8f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var death = other.GetComponent<PlayerDeath>();
            if (death != null)
            {
                Vector3 playerPos = other.transform.position;

                // Play particle effect at the fall location
                if (eliminationParticles != null)
                {
                    Instantiate(eliminationParticles, playerPos, Quaternion.identity);
                }

                // Play audio at the fall location
                if (eliminationSound != null)
                {
                    AudioSource.PlayClipAtPoint(eliminationSound, playerPos, soundVolume);
                }

                // Trigger respawn
                death.Respawn();
                Debug.Log("Player fell into elimination zone and was respawned.");
            }
        }
    }
}
