using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerCamera : MonoBehaviour
{
    // Sensitivity X Axis
    [SerializeField]
    private float sensX;
    // Sensitivity Y Axis
    [SerializeField]
    private float sensY;
    // Player Orientation Transform
    [SerializeField]
    private Transform playerOrientation;
    // Camera X Rotation
    private float xRotationCamera;
    // Camera Y Rotation
    private float yRotationCamera;
    // Mouse X
    private const string Mouse_X = "Mouse X";
    // Mouse Y
    private const string Mouse_Y = "Mouse Y";
    
    public Transform cameraHolder;

    private void Start()
    {
        LockMouseCursor();
    }

    private void Update()
    {
        RotateCamera();
    }

    /// <summary>
    /// Lock And Hide Mouse Cursor
    /// </summary>
    private void LockMouseCursor()
    {
        // Cursor Locked
        Cursor.lockState = CursorLockMode.Locked;
        // Cursor Hide
        Cursor.visible = false;
    }

    /// <summary>
    /// Rotate camera when mouse move in x or y axis
    /// </summary>
    private void RotateCamera()
    {
        // Get mouse X value and multiply in X sensitivity
        float mouseX = Input.GetAxisRaw(Mouse_X) * Time.deltaTime * sensX;
        // Get mouse Y value and multiply in Y sensitivity
        float mouseY = Input.GetAxisRaw(Mouse_Y) * Time.deltaTime * sensY;
        // Unity want to do that :)
        yRotationCamera += mouseX;
        xRotationCamera -= mouseY;
        // Make player lock up and down just to 90 or -90 degree
        xRotationCamera = Mathf.Clamp(xRotationCamera, -90f, 90f);
        // Rotate camera on both axis
        cameraHolder.rotation = Quaternion.Euler(xRotationCamera, yRotationCamera, 0);
        // Rotate player orientation along y axisx
        playerOrientation.rotation = Quaternion.Euler(0, yRotationCamera, 0);

    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
}