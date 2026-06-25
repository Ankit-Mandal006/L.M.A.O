using UnityEngine;

public class KnockbackObstacle : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] public float forceStrength = 15f;
    [SerializeField] public bool useDirectionFromObstacle = true;
    [SerializeField] public Vector3 customDirection = Vector3.forward;
    [SerializeField] public float verticalBoost = 5f;
    [SerializeField] public float cooldown = 0.2f;

    private float lastKnockbackTime = -10f;

    private void OnTriggerEnter(Collider other)
    {
        TryApplyKnockback(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryApplyKnockback(collision.gameObject);
    }

    private void TryApplyKnockback(GameObject target)
    {
        if (Time.time - lastKnockbackTime < cooldown) return;

        if (target.CompareTag("Player"))
        {
            var controller = target.GetComponent<ThirdPersonController>();
            if (controller != null)
            {
                lastKnockbackTime = Time.time;
                Vector3 knockbackDir = Vector3.zero;

                if (useDirectionFromObstacle)
                {
                    // Direction from center of this obstacle to player, flattened on Y axis
                    knockbackDir = (target.transform.position - transform.position);
                    knockbackDir.y = 0;
                    if (knockbackDir.sqrMagnitude < 0.001f)
                    {
                        knockbackDir = Vector3.forward;
                    }
                    else
                    {
                        knockbackDir.Normalize();
                    }
                }
                else
                {
                    // Use world-space direction transformed by obstacle rotation
                    knockbackDir = transform.TransformDirection(customDirection).normalized;
                }

                // Add vertical lift
                knockbackDir.y = 0;
                Vector3 finalForce = knockbackDir * forceStrength + Vector3.up * verticalBoost;

                controller.AddKnockback(finalForce);
                Debug.Log($"Applied knockback of {finalForce} to player.");
            }
        }
    }
}
