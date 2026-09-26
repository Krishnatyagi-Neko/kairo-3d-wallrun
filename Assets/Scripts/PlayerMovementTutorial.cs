using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float groundDrag = 5f;
    public float rotationSpeed = 15f;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    bool readyToJump = true;

    [Header("Dashing")]
    public float dashForce = 20f;
    public float dashCooldown = 1f;
    bool readyToDash = true;
    private float dashDuration = 0.2f;
    private float dashTimer = 0f;
    private bool isDashing = false;

    [Header("Crouching")]
    public float crouchSpeed = 2.5f;
    public float crouchHeight = 1f;
    public float normalHeight = 2f;
    public float crouchYOffset = 0.5f;
    bool isCrouching = false;
    private float normalYOffset;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    bool isGrounded;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode dashKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("References")]
    public Transform cameraTransform;
    public Rigidbody rb;

    private CapsuleCollider capsuleCollider;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        capsuleCollider = GetComponent<CapsuleCollider>();
        normalYOffset = capsuleCollider.center.y;

        // Player should only rotate through our script
        rb.freezeRotation = true;

        // Automatically find the Main Camera if not assigned
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        // Ground check
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            playerHeight * 0.5f + 0.2f,
            whatIsGround
        );

        HandleInput();
        HandlePlayerRotation();
        HandleDash();
        HandleCrouch();
        SpeedControl();

        // Apply drag
        if (isGrounded && !isDashing)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump
        if (Input.GetKeyDown(jumpKey) && readyToJump && isGrounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Dash
        if (Input.GetKeyDown(dashKey) && readyToDash && isGrounded)
        {
            readyToDash = false;

            StartDash();

            Invoke(nameof(ResetDash), dashCooldown);
        }

        // Crouch
        isCrouching = Input.GetKey(crouchKey);
    }

    private void HandlePlayerRotation()
    {
        if (cameraTransform == null)
            return;

        // Get camera's forward direction
        Vector3 cameraForward = cameraTransform.forward;

        // Ignore camera's vertical rotation
        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude < 0.01f)
            return;

        cameraForward.Normalize();

        // Camera's horizontal rotation
        Quaternion targetRotation = Quaternion.LookRotation(cameraForward);

        // Rotate player toward camera direction
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void MovePlayer()
    {
        if (isDashing)
            return;

        // Movement is now relative to the PLAYER,
        // which follows the camera's horizontal direction.
        moveDirection =
            transform.forward * verticalInput +
            transform.right * horizontalInput;

        // Prevent diagonal movement from being faster
        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        if (isGrounded)
        {
            rb.AddForce(
                moveDirection * moveSpeed * 10f,
                ForceMode.Force
            );
        }
        else
        {
            rb.AddForce(
                moveDirection * moveSpeed * 10f * airMultiplier,
                ForceMode.Force
            );
        }
    }

    private void HandleDash()
    {
        if (!isDashing)
            return;

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            isDashing = false;

            // Stop horizontal dash velocity
            rb.linearVelocity = new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );
        }
    }

    private void StartDash()
    {
        if (cameraTransform == null)
            return;

        isDashing = true;
        dashTimer = dashDuration;

        // Dash in camera's LOOK direction
        Vector3 dashDirection = cameraTransform.forward;

        // Don't dash into the floor/sky when looking up/down
        dashDirection.y = 0f;

        if (dashDirection.sqrMagnitude < 0.01f)
            dashDirection = transform.forward;

        dashDirection.Normalize();

        rb.linearVelocity = dashDirection * dashForce;
    }

    private void ResetDash()
    {
        readyToDash = true;
    }

    private void HandleCrouch()
    {
        if (isCrouching && !isDashing)
        {
            // Crouch
            if (capsuleCollider.height != crouchHeight)
            {
                capsuleCollider.height = crouchHeight;

                capsuleCollider.center = new Vector3(
                    0,
                    crouchYOffset,
                    0
                );
            }

            if (moveSpeed != crouchSpeed)
            {
                moveSpeed = crouchSpeed;
            }
        }
        else
        {
            // Stand up
            if (capsuleCollider.height != normalHeight)
            {
                capsuleCollider.height = normalHeight;

                capsuleCollider.center = new Vector3(
                    0,
                    normalYOffset,
                    0
                );
            }

            if (moveSpeed != 5f)
            {
                moveSpeed = 5f;
            }
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        float speedLimit = isCrouching ? crouchSpeed : moveSpeed;

        if (flatVel.magnitude > speedLimit)
        {
            Vector3 limitedVel = flatVel.normalized * speedLimit;

            rb.linearVelocity = new Vector3(
                limitedVel.x,
                rb.linearVelocity.y,
                limitedVel.z
            );
        }
    }

    private void Jump()
    {
        // Reset Y velocity before jumping
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        rb.AddForce(
            transform.up * jumpForce,
            ForceMode.Impulse
        );
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    // Getters for animation/UI
    public bool IsGrounded() => isGrounded;
    public bool IsCrouching() => isCrouching;
    public bool IsDashing() => isDashing;
}