using UnityEngine;
using Unity.Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float groundDrag = 5f;
    public float airMultiplier = 0.4f;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public float jumpCooldown = 0.25f;
    private bool readyToJump = true;

    [Header("Dashing")]
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private bool readyToDash = true;
    private bool isDashing = false;
    private float dashTimer;

    [Header("Wall Running")]
    public float wallRunSpeed = 8f;
    public float wallRunGravity = 1f;
    public float maxWallRunTime = 2f;
    public float wallCheckDistance = 0.8f;

    [Header("Wall Jump")]
    public float wallJumpForce = 5f;
    public float wallJumpAwayForce = 5f;

    private bool isWallRunning = false;
    private float wallRunTimer;
    private Vector3 wallNormal;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;

    private bool isGrounded;

    [Header("Wall Check")]
    public LayerMask whatIsWall;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode dashKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("References")]
    public Transform cameraTransform;
    public Rigidbody rb;

    [Header("Wall Run Camera")]
    public CinemachineCamera cinemachineCamera;
    public Transform cameraTarget;

    public float wallRunTilt = 12f;
    public float cameraTiltSpeed = 8f;
    public float wallCameraOffset = 0.2f;

private Vector3 cameraTargetStartPosition;

    private CapsuleCollider capsuleCollider;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;

    private float normalHeight;
    private float normalYOffset;

    private bool isCrouching = false;


    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        capsuleCollider = GetComponent<CapsuleCollider>();

        normalHeight = capsuleCollider.height;
        normalYOffset = capsuleCollider.center.y;

        rb.freezeRotation = true;

        cameraTargetStartPosition = cameraTarget.localPosition;
    }


    private void Update()
    {
        CheckGround();

        HandleInput();

        HandleDash();

        CheckWall();

        HandleWallRun();

        HandleWallRunCamera();

        HandleCrouch();

        if (!isDashing && !isWallRunning)
        {
            SpeedControl();
        }

        if (isGrounded && !isDashing && !isWallRunning)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0f;
        }
    }


    private void FixedUpdate()
    {
        if (isDashing)
            return;

        if (isWallRunning)
        {
            WallRunMovement();
            return;
        }

        MovePlayer();
    }


    // =========================================================
    // INPUT
    // =========================================================

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");


        // Jump
        if (Input.GetKeyDown(jumpKey) && readyToJump)
        {
            if (isGrounded)
            {
                readyToJump = false;
                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }
            else if (isWallRunning)
            {
                WallJump();
            }
        }


        // Dash
        if (Input.GetKeyDown(dashKey) && readyToDash)
        {
            StartDash();

            readyToDash = false;
            Invoke(nameof(ResetDash), dashCooldown);
        }


        // Crouch
        isCrouching = Input.GetKey(crouchKey);
    }


    // =========================================================
    // NORMAL MOVEMENT
    // =========================================================

    private void MovePlayer()
    {
        moveDirection =
            transform.forward * verticalInput +
            transform.right * horizontalInput;

        if (moveDirection.sqrMagnitude > 1f)
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


    // =========================================================
    // PLAYER ROTATION
    // =========================================================

    private void LateUpdate()
    {
        RotatePlayerWithCamera();
    }


    private void RotatePlayerWithCamera()
    {
        if (cameraTransform == null)
            return;

        Vector3 cameraForward = cameraTransform.forward;

        cameraForward.y = 0f;

        if (cameraForward.sqrMagnitude < 0.01f)
            return;

        cameraForward.Normalize();

        Quaternion targetRotation =
            Quaternion.LookRotation(cameraForward);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            15f * Time.deltaTime
        );
    }


    // =========================================================
    // JUMP
    // =========================================================

    private void Jump()
    {
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        rb.AddForce(
            Vector3.up * jumpForce,
            ForceMode.Impulse
        );
    }


    private void ResetJump()
    {
        readyToJump = true;
    }


    // =========================================================
    // DASH
    // =========================================================

    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;

        Vector3 dashDirection = cameraTransform.forward;

        // Don't dash upward/downward.
        dashDirection.y = 0f;

        if (dashDirection.sqrMagnitude < 0.01f)
            dashDirection = transform.forward;

        dashDirection.Normalize();

        rb.linearVelocity = new Vector3(
            dashDirection.x * dashForce,
            rb.linearVelocity.y,
            dashDirection.z * dashForce
        );
    }


    private void HandleDash()
    {
        if (!isDashing)
            return;

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            isDashing = false;

            // Stop horizontal dash velocity.
            rb.linearVelocity = new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );
        }
    }


    private void ResetDash()
    {
        readyToDash = true;
    }


    // =========================================================
    // WALL DETECTION
    // =========================================================

    private void CheckWall()
    {
        if (isGrounded || isDashing)
        {
            isWallRunning = false;
            return;
        }


        bool wallOnRight = Physics.Raycast(
            transform.position,
            transform.right,
            out RaycastHit rightHit,
            wallCheckDistance,
            whatIsWall
        );


        bool wallOnLeft = Physics.Raycast(
            transform.position,
            -transform.right,
            out RaycastHit leftHit,
            wallCheckDistance,
            whatIsWall
        );


        if (wallOnRight)
        {
            wallNormal = rightHit.normal;
            StartWallRun();
        }
        else if (wallOnLeft)
        {
            wallNormal = leftHit.normal;
            StartWallRun();
        }
        else
        {
            StopWallRun();
        }
    }


    // =========================================================
    // WALL RUN
    // =========================================================

    private void StartWallRun()
    {
        if (!isWallRunning)
        {
            isWallRunning = true;
            wallRunTimer = maxWallRunTime;
        }
    }


    private void HandleWallRun()
    {
        if (!isWallRunning)
            return;

        wallRunTimer -= Time.deltaTime;

        if (wallRunTimer <= 0f)
        {
            StopWallRun();
        }
    }


    private void WallRunMovement()
    {
        // Reduce gravity while wall running.
        rb.AddForce(
            Vector3.down * wallRunGravity,
            ForceMode.Force
        );


        // Find direction along the wall.
        Vector3 wallDirection =
            Vector3.Cross(Vector3.up, wallNormal);

        // Make sure we're running in the direction we're facing.
        if (Vector3.Dot(wallDirection, transform.forward) < 0f)
        {
            wallDirection = -wallDirection;
        }


        // Keep wall-run speed.
        rb.linearVelocity = new Vector3(
            wallDirection.x * wallRunSpeed,
            rb.linearVelocity.y,
            wallDirection.z * wallRunSpeed
        );
    }


    private void StopWallRun()
    {
        isWallRunning = false;
    }


    // =========================================================
    // WALL JUMP
    // =========================================================

    private void WallJump()
    {
        Vector3 jumpDirection =
            Vector3.up * wallJumpForce +
            wallNormal * wallJumpAwayForce;

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        rb.AddForce(
            jumpDirection,
            ForceMode.Impulse
        );

        StopWallRun();

        readyToJump = false;

        Invoke(nameof(ResetJump), jumpCooldown);
    }


    // =========================================================
    // GROUND CHECK
    // =========================================================

    private void CheckGround()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            playerHeight * 0.5f + 0.2f,
            whatIsGround
        );
    }


    // =========================================================
    // SPEED CONTROL
    // =========================================================

    private void SpeedControl()
    {
        Vector3 flatVelocity = new Vector3(
            rb.linearVelocity.x,
            0f,
            rb.linearVelocity.z
        );

        float speedLimit = isCrouching ? 2.5f : moveSpeed;

        if (flatVelocity.magnitude > speedLimit)
        {
            Vector3 limitedVelocity =
                flatVelocity.normalized * speedLimit;

            rb.linearVelocity = new Vector3(
                limitedVelocity.x,
                rb.linearVelocity.y,
                limitedVelocity.z
            );
        }
    }


    // =========================================================
    // CROUCH
    // =========================================================

    private void HandleCrouch()
    {
        if (isCrouching && !isDashing && !isWallRunning)
        {
            capsuleCollider.height = 1f;

            capsuleCollider.center =
                new Vector3(0f, 0.5f, 0f);
        }
        else
        {
            capsuleCollider.height = normalHeight;

            capsuleCollider.center =
                new Vector3(0f, normalYOffset, 0f);
        }
    }
    
    private void HandleWallRunCamera()
    {
        float targetTilt = 0f;

        Vector3 targetPosition = cameraTargetStartPosition;

        if (isWallRunning)
        {
            // Which side is the wall on?
            float wallSide = Vector3.Dot(
                wallNormal,
                transform.right
            );

            // Wall on LEFT
            if (wallSide > 0f)
            {
                targetTilt = -wallRunTilt;
            }
            // Wall on RIGHT
            else
            {
                targetTilt = +wallRunTilt;
            }

            // Move camera slightly AWAY from the wall.
            Vector3 offset = wallNormal * wallCameraOffset;

            targetPosition += transform.InverseTransformVector(offset);
        }

        // Smooth camera roll
        cinemachineCamera.Lens.Dutch = Mathf.Lerp
        (
            cinemachineCamera.Lens.Dutch,
            targetTilt,
            cameraTiltSpeed * Time.deltaTime
        );

        // Smooth camera position
        cameraTarget.localPosition = Vector3.Lerp(
            cameraTarget.localPosition,
            targetPosition,
            cameraTiltSpeed * Time.deltaTime
        );
    }

    // =========================================================
    // GETTERS
    // =========================================================

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    public bool IsWallRunning()
    {
        return isWallRunning;
    }

    public bool IsCrouching()
    {
        return isCrouching;
    }
}