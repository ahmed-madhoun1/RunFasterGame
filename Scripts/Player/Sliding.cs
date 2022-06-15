using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform playerOrientation;
    public Transform playerObject;
    private Rigidbody playerRigidbody;
    private PlayerMovementAdvanced playerMovementAdvancedScript;

    [Header("Sliding")]
    public float maxSlideTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;


    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerMovementAdvancedScript = GetComponent<PlayerMovementAdvanced>();

        // Save player Y scale for later
        startYScale = playerObject.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Check if slide key down and pressing one of the four movement keys
        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
            StartSlide();

        // Check if slide key up and player is isPlayerSliding
        if (Input.GetKeyUp(slideKey) && playerMovementAdvancedScript.isPlayerSliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (playerMovementAdvancedScript.isPlayerSliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        playerMovementAdvancedScript.isPlayerSliding = true;
        // Change player Y scale when he isPlayerSliding
        playerObject.localScale = new Vector3(playerObject.localScale.x, slideYScale, playerObject.localScale.z);
        // Because when player shrink his Y scale, he floating in the air a bit so i added a force to push player to the ground
        playerRigidbody.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        // Reset slide timer
        slideTimer = maxSlideTime;
    }

    private void SlidingMovement()
    {
        // Calculate input direction
        // This way player can slide in all directions depending on which keys is pressing
        Vector3 inputDirection = playerOrientation.forward * verticalInput + playerOrientation.right * horizontalInput;

        // isPlayerSliding normal
        // Check if player not on slope or not moving upwards
        if (!playerMovementAdvancedScript.OnSlope() || playerRigidbody.velocity.y > -0.1f)
        {
            // Apply force in calculating direction
            playerRigidbody.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
            // Count down slide timer
            slideTimer -= Time.deltaTime;
        }

        // isPlayerSliding down a slope
        else
        {
            playerRigidbody.AddForce(playerMovementAdvancedScript.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        // Stop isPlayerSliding when slideTimer = 0
        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        playerMovementAdvancedScript.isPlayerSliding = false;
        // Change player Y scale after stop his isPlayerSliding
        playerObject.localScale = new Vector3(playerObject.localScale.x, startYScale, playerObject.localScale.z);
    }
}
