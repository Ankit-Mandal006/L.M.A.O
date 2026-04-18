using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Normal walking speed")]
    public float walkSpeed = 5f;
    
    [Tooltip("Running speed when holding shift")]
    public float runSpeed = 8f;
    
    [Tooltip("How fast the player accelerates")]
    public float acceleration = 10f;
    
    [Tooltip("How fast the player decelerates")]
    public float deceleration = 10f;
    
    [Tooltip("How fast the player rotates to face movement direction")]
    public float rotationSpeed = 10f;

    [Header("Jump Settings")]
    [Tooltip("Jump height")]
    public float jumpHeight = 2f;
    
    [Tooltip("Gravity multiplier")]
    public float gravity = 20f;
    
    [Tooltip("Number of jumps allowed (1 = single jump, 2 = double jump, etc.)")]
    public int maxJumps = 1;

    [Header("Ground Check")]
    [Tooltip("Distance to check for ground")]
    public float groundCheckDistance = 0.2f;
    
    [Tooltip("Layer mask for ground detection")]
    public LayerMask groundMask = 1;

    [Header("Camera Settings")]
    [Tooltip("Reference to the camera that follows the player")]
    public Transform playerCamera;
    
    [Tooltip("Camera follow smoothness")]
    public float cameraSmoothness = 10f;

    // Private variables
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 currentMovement;
    private bool isGrounded;
    private int jumpCount;
    private float currentSpeed;

    // Input
    private float horizontalInput;
    private float verticalInput;
    private bool jumpInput;
    private bool runInput;

    [Header("Advanced Gravity")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // If no camera assigned, find main camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }
        
        // Lock cursor for better gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Get input
        GetInput();
        
        // Check if grounded
        CheckGround();
        
        // Handle movement
        HandleMovement();
        
        // Handle jumping
        HandleJump();
        
        // Apply gravity
        ApplyGravity();
        
        // Move the character
        controller.Move(velocity * Time.deltaTime);
        
        // Unlock cursor with Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        jumpInput = Input.GetButtonDown("Jump");
        runInput = Input.GetKey(KeyCode.LeftShift);
    }

    private void CheckGround()
{
    // Use built-in grounded check (most stable)
    isGrounded = controller.isGrounded;

    // Extra raycast for accuracy (prevents edge bugs)
    if (!isGrounded)
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        isGrounded = Physics.Raycast(ray, groundCheckDistance + 0.1f, groundMask);
    }

    // Reset when grounded
    if (isGrounded && velocity.y < 0)
    {
        velocity.y = -2f;
        jumpCount = 0;
    }
}

    private void HandleMovement()
    {
        // Get camera forward and right directions
        Vector3 cameraForward = playerCamera.forward;
        Vector3 cameraRight = playerCamera.right;
        
        // Flatten camera directions (ignore Y)
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // Calculate movement direction relative to camera
        Vector3 moveDirection = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;

        // Determine target speed
        float targetSpeed = runInput ? runSpeed : walkSpeed;
        
        // If there's input, accelerate
        if (moveDirection.magnitude > 0.1f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            currentMovement = moveDirection * currentSpeed;
            
            // Rotate player to face movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Decelerate when no input
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, deceleration * Time.deltaTime);
            currentMovement = Vector3.Lerp(currentMovement, Vector3.zero, deceleration * Time.deltaTime);
        }

        // Apply horizontal movement
        velocity.x = currentMovement.x;
        velocity.z = currentMovement.z;
    }

    private void HandleJump()
    {
        if (jumpInput && jumpCount < maxJumps)
        {
        // Reset downward velocity before jump (important)
            if (velocity.y < 0)
                velocity.y = 0f;

            velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
            jumpCount++;
        }
    }

    private void ApplyGravity()
{
    if (isGrounded)
    {
        if (velocity.y < 0)
            velocity.y = -2f; // stick to ground
    }
    else
    {
        velocity.y -= gravity * Time.deltaTime;
    }
}

    // Public methods for external scripts (like TrollTrigger)
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw ground check sphere
        Gizmos.color = Color.red;
        Vector3 spherePosition = transform.position + Vector3.down * (GetComponent<CharacterController>() ? GetComponent<CharacterController>().height / 2 : 1f);
        Gizmos.DrawWireSphere(spherePosition, groundCheckDistance);
    }
}