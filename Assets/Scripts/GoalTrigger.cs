using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GoalTrigger : MonoBehaviour
{
    [Header("Settings")]
    public string playerTag = "Player";
    public string sceneToLoad; // exact scene name

    [Header("Cinematic Orbit Settings")]
    [Tooltip("How long the camera revolves around the player before loading the scene.")]
    public float winSequenceDuration = 4.0f;
    [Tooltip("Degrees per second the camera rotates around the player.")]
    public float orbitSpeed = 50.0f;
    [Tooltip("How far away the camera sits from the player during orbit.")]
    public float cameraRadius = 6.0f;
    [Tooltip("How high up the camera rests relative to the player's position.")]
    public float cameraHeight = 2.5f;
    [Tooltip("How fast the camera smoothly blends from its current position into the orbit path.")]
    public float cameraLerpSpeed = 4.0f;

    [Header("Player Control Settings")]
    [Tooltip("The name of your movement script component so it can be disabled on win.")]
    public string movementScriptName = "PlayerController";

    private Transform mainCameraTransform;
    private Transform playerTransform;
    private bool isWinning = false;
    private float currentAngle = 0.0f;
    private bool isBlendingToOrbit = true;

    private void Start()
    {
        // Cache the main camera safely at start
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("GoalTrigger: Ensure your main camera is tagged as 'MainCamera' in the inspector!", this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Block multiple trigger executions if player bounces inside the zone
        if (isWinning) return;

        if (other.CompareTag(playerTag))
        {
            playerTransform = other.transform;
            StartWinSequence();
        }
    }

    private void StartWinSequence()
    {
        isWinning = true;
        isBlendingToOrbit = true;

        // 1. Shut down any standard tracking scripts on the camera (like Cinemachine, look-ats, or follow behaviors)
        MonoBehaviour[] cameraScripts = mainCameraTransform.GetComponents<MonoBehaviour>();
        foreach (var script in cameraScripts)
        {
            // Don't disable *this* script if it happens to live on the camera
            if (script != this) 
            {
                script.enabled = false;
            }
        }

        // 2. Shut off player controls immediately so they cannot move
        var playerControl = playerTransform.GetComponent(movementScriptName) as MonoBehaviour;
        if (playerControl != null)
        {
            playerControl.enabled = false;
            Debug.Log($"GoalTrigger: Disabled {movementScriptName} component successfully.");
        }
        else
        {
            Debug.LogWarning($"GoalTrigger: Could not find a script named '{movementScriptName}' on the player! Check your spelling.");
        }

        // 3. Freeze player physics momentum completely so they drop or stop instantly
        Rigidbody playerRb = playerTransform.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.isKinematic = true; 
        }

        // 4. Calculate the starting angle based on where the camera is relative to the player right now
        Vector3 displacement = mainCameraTransform.position - playerTransform.position;
        displacement.y = 0; // Flatten vectors to find the horizontal angle
        currentAngle = Mathf.Atan2(displacement.z, displacement.x) * Mathf.Rad2Deg;

        // 5. Fire off the countdown timer to load the next scene
        StartCoroutine(TimedSceneTransition());
    }

    private void LateUpdate()
    {
        // Process camera calculations in LateUpdate to prevent any jittering
        if (isWinning && playerTransform != null && mainCameraTransform != null)
        {
            // Step the angle forward based on time
            currentAngle += orbitSpeed * Time.deltaTime;
            float radians = currentAngle * Mathf.Deg2Rad;

            // Calculate the exact target coordinate on the mathematical circle circumference
            float xPositionOffset = Mathf.Cos(radians) * cameraRadius;
            float zPositionOffset = Mathf.Sin(radians) * cameraRadius;

            Vector3 targetOrbitPosition = new Vector3(
                playerTransform.position.x + xPositionOffset,
                playerTransform.position.y + cameraHeight,
                playerTransform.position.z + zPositionOffset
            );

            // Calculate target look rotation (aiming slightly up towards player center mass)
            Vector3 targetLookAtPoint = playerTransform.position + Vector3.up * 1.0f;
            Quaternion targetOrbitRotation = Quaternion.LookRotation(targetLookAtPoint - mainCameraTransform.position);

            if (isBlendingToOrbit)
            {
                // Smoothly Lerp position and Slerp rotation from current gameplay camera state into the orbit track
                mainCameraTransform.position = Vector3.Lerp(mainCameraTransform.position, targetOrbitPosition, Time.deltaTime * cameraLerpSpeed);
                mainCameraTransform.rotation = Quaternion.Slerp(mainCameraTransform.rotation, targetOrbitRotation, Time.deltaTime * cameraLerpSpeed);

                // Once we are close enough to the mathematical track, stop lerping and hard lock onto it
                if (Vector3.Distance(mainCameraTransform.position, targetOrbitPosition) < 0.1f)
                {
                    isBlendingToOrbit = false;
                }
            }
            else
            {
                // Hard-locked tracking state once the transition blend finishes
                mainCameraTransform.position = targetOrbitPosition;
                mainCameraTransform.LookAt(targetLookAtPoint);
            }
        }
    }

    private IEnumerator TimedSceneTransition()
    {
        // Let the camera spin loop for our set time duration
        yield return new WaitForSeconds(winSequenceDuration);
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("GoalTrigger: 'sceneToLoad' field is left unassigned in the inspector!", this);
        }
    }
}