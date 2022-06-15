using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerMovementAdvanced : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float sprintSpeed;
    [SerializeField]
    private float slideSpeed;
    [SerializeField]
    private float wallrunSpeed;
    [SerializeField]
    private float speedIncreaseMultiplier;
    [SerializeField]
    private float slopeIncreaseMultiplier;
    [SerializeField]
    private float groundDrag;

    [Header("Jumping")]
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float jumpCooldown;
    [SerializeField]
    private float airMultiplier;
    private bool readyToJump;

    [Header("Crouching")]
    [SerializeField]
    private float crouchSpeed;
    [SerializeField]
    private float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    [SerializeField]
    private KeyCode jumpKey = KeyCode.Space;
    [SerializeField]
    private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField]
    private KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    [SerializeField]
    private float playerHeight;
    [SerializeField]
    private LayerMask whatIsGround;
    private bool grounded;

    [Header("Slope Handling")]
    [SerializeField]
    private float maxSlopeAngle;
    private RaycastHit slopeHit;
    /// <summary>
    /// When player is on the slope his speed (whole playerRigidbody.velocity) is limited
    /// so the jump (playerRigidbody.velocity.y) is limiting, example: instead of jumping 10 he jumped 2
    /// </summary>
    private bool isPlayerExitingSlope;
    [SerializeField]
    private Transform playerOrientation;

    [Header("Gun Shooting")]
    public Gun gunScript;
    [SerializeField]
    private int playerDamage = 5;
    [SerializeField]
    private GunPick gunPick;
    [Header("Audios")]
    [SerializeField]
    private AudioSource PistolGunAudioSource;
    [SerializeField]
    private AudioSource ShootGunAudioSource;
    [SerializeField]
    private AudioSource MachineGunAudioSource;

    /// <summary>
    /// adding new
    /// </summary>
    public bool isPlayerKilled = false;
    public int health;
    public Animator playerCamAnimator;
    public PauseMenu pauseMenu;
    public HealthBar healthBar;
    [SerializeField] private Transform playerCameraTransform;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDirection;

    private Rigidbody playerRigidbody;


    // Store player movement current state
    [SerializeField]
    private MovementState state;
    public bool isPlayerSliding;
    public bool crouching;
    public bool wallrunning;

    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        Sliding,
        air
    }
    private void Start()
    {
        healthBar.SetMaxHealth(health);
        // Init player rigidbody
        playerRigidbody = GetComponent<Rigidbody>();
        // Freeze player rigidbody rotation
        playerRigidbody.freezeRotation = true;
        // Set player normal Y scale
        startYScale = transform.localScale.y;

        readyToJump = true;
        
    }

    private void Update()
    {
        // Check if player on the ground
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MovementKeys();
        SpeedControl();
        PlayerMovementStateHandler();
        HandleDrag();

    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    /// <summary>
    /// Init horizontal and vertical movement keys
    /// </summary>
    private void MovementKeys()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jumping
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Start Crouch
        if (Input.GetKeyDown(crouchKey) && horizontalInput == 0 && verticalInput == 0)
        {
            // Change player Y scale when he crouching
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            // Because when player crouching, he floating in the air a bit so i added a force to push player to the ground
            playerRigidbody.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            crouching = true;
        }

        // Stop Crouch
        if (Input.GetKeyUp(crouchKey))
        {
            // Set normal crouch Y scale value
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            crouching = false;
        }

        // gunScript Shooting
        var input = gunScript.gunType == Gun.GunType.MachineGun ? Input.GetMouseButton(0) : Input.GetMouseButtonDown(0);
        if (input && gunPick.IsPlayerPickedGun && gunPick.currentGun.tag == "Gun" && Time.timeScale != 0)
        {
            //It can be useful to limit the maximum rate of fire with separate presses. In my opinion, an already rapid weapon
            //(repeat time <= 0.1) will sound messy when the key is stroked rapidly, if the key press firing rate is not limited.

        
            switch (gunScript.gunType)
            {
                case Gun.GunType.Pistol:
                    PistolGunAudioSource.Play();
                    gunScript.Shoot();
                    break;
                case Gun.GunType.Shotgun:
                    ShootGunAudioSource.Play();
                    gunScript.Shoot();
                    break;
                case Gun.GunType.MachineGun:
                    if (input)
                    {
                        MachineGunAudioSource.Play();
                    }
                    gunScript.Shoot();
                    break;
            }
        }
    }

    /// <summary>
    /// Check when the player is walking, sprinting, crouching or in the air
    /// </summary>
    private void PlayerMovementStateHandler()
    {
        // Mode - Wallrunning
        if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;
        }

        // Mode - Sliding
        else if (isPlayerSliding)
        {
            state = MovementState.Sliding;

            // Check if player on slope and moving downwards
            if (OnSlope() && playerRigidbody.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;

            else
                desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Crouching
        else if (crouching)
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        // Check if desiredMoveSpeed has changed drastically
        // Check the difference between desiredMoveSpeed and lastDesiredMoveSpeed if gratter than 4
        // Example: if you are changing from walking to sprinting the speed difference is only 3 therefor 
        // the speed changes instantly but if you build up speed like 30 and you changing to sprinting 
        // the difference is 20 which is grater than 4 which mean the speed will now slowly decrease
        // on one side you can quickly changed between sprinting and walking and on the other side
        // you slowly change between going realyly fast and really slow
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());

            print("Lerp Started!");
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }
        // Save last desired move speed
        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    /// <summary>
    /// Changing move speed to desired move speed over time (smoothly)
    /// </summary>
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // Smoothly lerp movement speed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                // Build up more speed depending on how steep 
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    /// <summary>
    /// Calculate player direction and move him to it
    /// </summary>
    private void MovePlayer()
    {
        // This way player alwayes walk in the direction he looking
        moveDirection = playerOrientation.forward * verticalInput + playerOrientation.right * horizontalInput;

        // If the player is on the slope and not exiting the slop (not jumping)
        if (OnSlope() && !isPlayerExitingSlope)
        {
            // then add force in the slope move direction we just calculated
            playerRigidbody.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);
            // Becase when gravity turn off the player just jumping when move on the slope
            // so i add a force to push player down if the y more than 0 (if he jump)
            if (playerRigidbody.velocity.y > 0)
                playerRigidbody.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // If the player is on the ground, add force to player rigidbody
        // and make it faster by multiply moveDirection with moveSpeed
        else if (grounded)
            playerRigidbody.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // If the player is not on the ground, add force to player rigidbody
        // and make it faster by multiply moveDirection with moveSpeed and airMultiplier
        else if (!grounded)
            playerRigidbody.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // Turn gravity off while on slope because the player moving down when he stop on slope
        if (!wallrunning) playerRigidbody.useGravity = !OnSlope();
    }
    /// <summary>
    /// Check if player grounded to make his rigidbody drag
    /// </summary>
    private void HandleDrag()
    {
        if (grounded)
            playerRigidbody.drag = groundDrag;
        else
            playerRigidbody.drag = 0;
    }

    /// <summary>
    /// Limit player speed to the given value
    /// </summary>
    private void SpeedControl()
    {
        // Limiting speed on the slope
        // If the player is on the slope and not exiting the slop (not jumping)
        if (OnSlope() && !isPlayerExitingSlope)
        {
            if (playerRigidbody.velocity.magnitude > moveSpeed)
                playerRigidbody.velocity = playerRigidbody.velocity.normalized * moveSpeed;
        }

        // Limiting speed on the ground or in the air
        else
        {
            Vector3 flatVel = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);

            // if player go faster than the movement speed
            if (flatVel.magnitude > moveSpeed)
            {
                // Calculate the max velocity would be
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                // Apply the new velocity
                playerRigidbody.velocity = new Vector3(limitedVel.x, playerRigidbody.velocity.y, limitedVel.z);
            }
        }
    }
    /// <summary>
    /// Make player jump
    /// </summary>
    private void Jump()
    {
        isPlayerExitingSlope = true;

        // Set Y velocity 0, This way the player will alwayes jump the exact same height
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);
        // Apply the force in the up direction multiplied with jumpForce and using ForceMode.Impulse to apply force once
        playerRigidbody.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    /// <summary>
    /// Reset jump
    /// </summary>
    private void ResetJump()
    {
        readyToJump = true;

        isPlayerExitingSlope = false;
    }

    /// <summary>
    /// Check if the player on the slope
    /// </summary>
    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            // Calculate how steep the slope per standard is
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            // return true if the angle is smaller than the max slope angle (that means player can move on that slope) and
            // angle not zero (there is a slope)
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    /// <summary>
    /// Projected the normal move direction into the slope to find the correct direction relative to our slope 
    /// before  __|
    /// after   \/
    /// </summary>
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }

 //  private void OnTriggerEnter(Collider collider)
 // {
 //      if (collider.gameObject.tag == "EnemyPolit")
 //      {
 //           TakeDamage();
 //       }
 //  }

    public void TakeDamage()
    {
        health -= playerDamage;
        healthBar.SetHealth(health);
        if (health <= 0) Invoke(nameof(DestroyPlayer), 0.5f);
    }
    public void DestroyPlayer()
    {
        if (!isPlayerKilled)
        {
            /// Change camera position to the last position was player in
            playerCameraTransform.parent.position = transform.position;
            transform.localPosition = Vector3.zero;
            if (gunPick.IsPlayerPickedGun)
                gunPick.DropGun();
            transform.Rotate(90, transform.rotation.y, transform.rotation.z);
            playerCamAnimator.enabled = true;
            playerCamAnimator.Play("CameraAnimation");
            isPlayerKilled = true;
            this.enabled = false;
            pauseMenu.PlayerKilled();
            Debug.Log("Player Killed!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "DeadFloor")
        {
            DestroyPlayer();
        }
    }

    public void GetBackMoveSpeed()
    {
        moveSpeed = walkSpeed;
    }
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
}
