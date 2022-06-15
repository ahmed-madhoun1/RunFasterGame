using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunningAdvanced : MonoBehaviour
{
    [Header("Wall Running")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode upwardsRunKey = KeyCode.LeftShift;
    public KeyCode downwardsRunKey = KeyCode.LeftControl;
    private bool upwardsRunning;
    private bool downwardsRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Wall Detection")]
    public float wallCheckDistance;
    // The min value for player height to do wall run
    public float minJumpHeight;
    private RaycastHit leftWallhit;
    private RaycastHit rightWallhit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall;
    public float exitWallTime;
    private float exitWallTimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform playerOrientation;
    public PlayerCamera playerCamera;
    private PlayerMovementAdvanced playerMovementAdvancedScript;
    private Rigidbody playerRigidbody;

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerMovementAdvancedScript = GetComponent<PlayerMovementAdvanced>();
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if (playerMovementAdvancedScript.wallrunning)
            WallRunningMovement();
    }


    /// <summary>
    /// Check if player hit a wall from left or right
    /// </summary>
    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, playerOrientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -playerOrientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
    }

    /// <summary>
    /// Check if player is high enough in the air to start wall runing
    /// </summary>
    private bool AboveGround()
    {
        // return true if the raycast hit nothing
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    /// <summary>
    /// Define when the player should enter the wall runing state   
    /// </summary>
    private void StateMachine()
    {
        // Getting Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        upwardsRunning = Input.GetKey(upwardsRunKey);
        downwardsRunning = Input.GetKey(downwardsRunKey);

        // State 1 - Wallrunning
        // Check if there is a wall on the left or right side for the player,
        // and pressing the (W) key (the key to moving forward), and he is above the ground, 
        // and he is not exiting the wall
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            if (!playerMovementAdvancedScript.wallrunning)
                StartWallRun();

            // wallrun timer
            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;

            if(wallRunTimer <= 0 && playerMovementAdvancedScript.wallrunning)
            {
                exitingWall = true;
                exitWallTimer = exitWallTime;
            }

            // wall jump
            if (Input.GetKeyDown(jumpKey)) WallJump();
        }

        // State 2 - Exiting
        // Check if player exiting the wall
        else if (exitingWall)
        {
            if (playerMovementAdvancedScript.wallrunning)
                StopWallRun();

            if (exitWallTimer > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                exitingWall = false;
        }

        // State 3 - None
        else
        {
            if (playerMovementAdvancedScript.wallrunning)
                StopWallRun();
        }
    }

    /// <summary>
    /// Start wall run
    /// </summary>
    private void StartWallRun()
    {
        playerMovementAdvancedScript.wallrunning = true;
        // Set wall run timer
        wallRunTimer = maxWallRunTime;
        // Add velocity to player rigidbody except y axis
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);
        // Apply camera effects
        playerCamera.DoFov(90f);
        if (wallLeft) playerCamera.DoTilt(-5f);
        if (wallRight) playerCamera.DoTilt(5f);
    }

    /// <summary>
    /// Wall Running Movement
    /// </summary>
    private void WallRunningMovement()
    {
        // Toggle use gravity
        playerRigidbody.useGravity = useGravity;
        // Vector3.Cross to find the forward direction of the wall because this has  to work no matter how the wall is rotated,
        // this function will take right or left and upwards direction and then return the forward direction
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        // To make wall run working from the two sides because it's just working in one side
        // and when the player try from the other side he will wall running backwards and To find out which
        // direction is closer to where the player is facing and if the player is on the other side
        // to change the (wallForward) to (-wallForward) the code here will fix this
        if ((playerOrientation.forward - wallForward).magnitude > (playerOrientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // Add forward force
        playerRigidbody.AddForce(wallForward * wallRunForce, ForceMode.Force);
        // Add upwards/downwards force to run upwards or downwards when player running on the wall
        if (upwardsRunning)
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, wallClimbSpeed, playerRigidbody.velocity.z);
        if (downwardsRunning)
            playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, -wallClimbSpeed, playerRigidbody.velocity.z);

        // Push to wall force because when the player try to wall run on the outside of a curved wall he lose contact
        // Check if the player not currently trying to get away from the wall by checking if there is a wall in the
        // left and he is not pressing (Right move key) and there is a wall on the right and he is not pressing (Left move key)
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            playerRigidbody.AddForce(-wallNormal * 100, ForceMode.Force);

        // weaken gravity
        if (useGravity)
            playerRigidbody.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    /// <summary>
    /// Stop Wall Run
    /// </summary>
    private void StopWallRun()
    {
        playerMovementAdvancedScript.wallrunning = false;
        // Rreset camera effects
        playerCamera.DoFov(80f);
        playerCamera.DoTilt(0f);
    }

    /// <summary>
    /// Wall jump
    /// </summary>
    private void WallJump()
    {
        // Enter exiting wall state
        exitingWall = true;
        exitWallTimer = exitWallTime;

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // reset y velocity and add force
        playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0f, playerRigidbody.velocity.z);
        playerRigidbody.AddForce(forceToApply, ForceMode.Impulse);
    }
}
