using UnityEngine;

public class MobileInputController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("The main player object with the ThirdPersonController script attached")]
    public ThirdPersonController playerController;

    [Header("Virtual Joystick Settings")]
    [Tooltip("The background/container UI element of your joystick")]
    public RectTransform joystickBackground;
    [Tooltip("The inner handle element of your joystick that slides around")]
    public RectTransform joystickHandle;
    [Tooltip("Maximum pixel distance the handle can move from the center")]
    public float joystickRange = 100f;

    // Input tracking variables
    private Vector2 moveInput = Vector2.zero;
    private int joystickTouchId = -1;
    private Vector2 joystickCenter = Vector2.zero;

    private void Start()
    {
        if (playerController == null)
        {
            playerController = GetComponent<ThirdPersonController>();
        }

        // Ensure mouse cursor restrictions from desktop don't block touch UI inputs
        #if UNITY_ANDROID || UNITY_IOS
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        #endif

        if (joystickBackground != null)
        {
            joystickCenter = joystickBackground.position;
        }
    }

    private void Update()
    {
        HandleTouchInput();
        
        // Push the values into the ThirdPersonController script
        if (playerController != null)
        {
            playerController.SetMobileInput(moveInput.x, moveInput.y);
        }
    }

    private void HandleTouchInput()
    {
        // Loop through all active touches on the mobile screen
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                // Check if the touch happened inside the boundaries of our virtual joystick
                if (joystickTouchId == -1 && RectTransformUtility.RectangleContainsScreenPoint(joystickBackground, touch.position))
                {
                    joystickTouchId = touch.fingerId;
                }
            }

            if (touch.fingerId == joystickTouchId)
            {
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    // Calculate the offset distance from the center of the joystick UI container
                    Vector2 offset = touch.position - joystickCenter;
                    offset = Vector2.ClampMagnitude(offset, joystickRange);

                    // Position the joystick handle element visually
                    if (joystickHandle != null)
                    {
                        joystickHandle.position = joystickCenter + offset;
                    }

                    // Normalize values between -1 and 1 to simulate simulated Input.GetAxisRaw()
                    moveInput = offset / joystickRange;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    // Reset everything to center when the finger leaves the joystick zone
                    ResetJoystick();
                }
            }
        }
    }

    private void ResetJoystick()
    {
        joystickTouchId = -1;
        moveInput = Vector2.zero;
        if (joystickHandle != null && joystickBackground != null)
        {
            joystickHandle.position = joystickBackground.position;
        }
    }

    /// <summary>
    /// Connect this public function directly to your UI Jump Button component's OnClick() list
    /// </summary>
    public void OnJumpButtonPressed()
    {
        if (playerController != null)
        {
            playerController.TriggerMobileJump();
        }
    }
}