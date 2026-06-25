using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    public float rotationSpeed = 10f;

    [Header("Mobile Settings")]
    [Range(0.1f, 3f)]
    public float mobileSensitivity = 1.5f;

    [Header("Jump Settings")]
    public float jumpHeight = 2f;
    public float gravity = 20f;
    public int maxJumps = 1;

    [Header("Ground Check")]
    public float groundCheckDistance = 0.2f;
    public LayerMask groundMask = 1;

    [Header("Camera Settings")]
    public Transform playerCamera;
    public float cameraSmoothness = 10f;

    [Header("Advanced Gravity")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 currentMovement;
    private bool isGrounded;
    private int jumpCount;
    private float currentSpeed;

    private float horizontalInput;
    private float verticalInput;
    private bool jumpInput;
    private bool runInput;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackDecay = 5f;
    private Vector3 knockbackForce = Vector3.zero;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        maxJumps = 1; // Explicitly disable double jump by forcing maxJumps to 1
        
        if (playerCamera == null)
        {
            playerCamera = Camera.main.transform;
        }
        
        #if !UNITY_ANDROID && !UNITY_IOS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        #endif
    }

    private void Update()
    {
        #if UNITY_EDITOR
        if (Input.touches.Length == 0)
        {
            GetInput();
        }
        #elif !UNITY_ANDROID && !UNITY_IOS
        GetInput();
        #endif
        
        CheckGround();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        
        Vector3 finalMove = velocity;
        if (knockbackForce.sqrMagnitude > 0.01f)
        {
            finalMove += knockbackForce;
            knockbackForce = Vector3.Lerp(knockbackForce, Vector3.zero, knockbackDecay * Time.deltaTime);
        }
        else
        {
            knockbackForce = Vector3.zero;
        }

        controller.Move(finalMove * Time.deltaTime);

        jumpInput = false;

        #if !UNITY_ANDROID && !UNITY_IOS
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        #endif
    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        if (Input.GetButtonDown("Jump"))
        {
            jumpInput = true;
        }
        
        runInput = Input.GetKey(KeyCode.LeftShift);
    }

    private void CheckGround()
    {
        isGrounded = controller.isGrounded;

        if (!isGrounded)
        {
            isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance + 0.1f, groundMask);
        }

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            jumpCount = 0;
        }
    }

    private void HandleMovement()
    {
        Vector3 cameraForward = playerCamera.forward;
        Vector3 cameraRight = playerCamera.right;
        
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;
        float targetSpeed = runInput ? runSpeed : walkSpeed;
        
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            currentMovement = moveDirection * currentSpeed;
            
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, deceleration * Time.deltaTime);
            currentMovement = Vector3.Lerp(currentMovement, Vector3.zero, deceleration * Time.deltaTime);
        }

        velocity.x = currentMovement.x;
        velocity.z = currentMovement.z;
    }

    private void HandleJump()
    {
        if (jumpInput && jumpCount < maxJumps)
        {
            if (velocity.y < 0)
            {
                velocity.y = 0f;
            }

            velocity.y = Mathf.Sqrt(jumpHeight * 2f * gravity);
            jumpCount++;
        }
    }

    private void ApplyGravity()
    {
        if (isGrounded)
        {
            if (velocity.y < 0)
            {
                velocity.y = -2f;
            }
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }
    }

    public void AddKnockback(Vector3 force)
    {
        knockbackForce += force;
    }

    public void Launch(float upwardForce)
    {
        velocity.y = upwardForce;
    }

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

    public void SetMobileInput(float horizontal, float vertical)
    {
        horizontalInput = Mathf.Clamp(horizontal * mobileSensitivity, -1f, 1f);
        verticalInput = Mathf.Clamp(vertical * mobileSensitivity, -1f, 1f);
    }

    public void TriggerMobileJump()
    {
        jumpInput = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        float height = controller != null ? controller.height : 2f;
        Vector3 spherePosition = transform.position + Vector3.down * (height / 2f);
        Gizmos.DrawWireSphere(spherePosition, groundCheckDistance);
    }
}