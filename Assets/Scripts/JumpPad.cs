using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [Header("Jump Pad Settings")]
    [SerializeField] public float launchForce = 15f;
    [SerializeField] public ParticleSystem launchParticles;
    [SerializeField] public AudioSource launchAudio;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var controller = other.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                controller.Launch(launchForce);

                // Play feedback effects
                if (launchParticles != null)
                {
                    launchParticles.Play();
                }

                if (launchAudio != null)
                {
                    launchAudio.Play();
                }

                Debug.Log($"JumpPad: Launched player with force {launchForce}");
            }
        }
    }
}
