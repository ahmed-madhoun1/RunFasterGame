using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPick : MonoBehaviour
{
    [SerializeField]
    private Transform gunPosition;
    [SerializeField]
    private float distance = 10;
    [SerializeField]
    private Transform playerCamera;
    [SerializeField]
    private PlayerMovementAdvanced playerMovementAdvanced;
    private bool isGun = false;
    public GameObject currentGun;
    private GameObject newGun;
    public bool IsPlayerPickedGun;

    private void Update()
    {
        CheckGun();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isGun)
            {
                PickUpGun();
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (IsPlayerPickedGun)
            {
                DropGun();
            }
        }

    }

    private void CheckGun()
    {
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, out hit, distance))
        {
            if(hit.transform.gameObject.tag == "Gun" || hit.transform.gameObject.tag == "GrapplingGun")
            {
                Debug.Log("CheckGun => " + hit.transform.gameObject.tag);
                newGun = hit.transform.gameObject;
                isGun = true;
            }
        }
    }

    private void PickUpGun()
    {
        if (IsPlayerPickedGun)
        {
            DropGun();
        }
        currentGun = newGun;
        Debug.Log("PickUpGun => " + currentGun.transform.gameObject.name);
        playerMovementAdvanced.gunScript = currentGun.GetComponent<Gun>();
        currentGun.transform.parent = gunPosition.transform;
        currentGun.transform.localPosition = gunPosition.transform.localPosition;
        currentGun.GetComponent<Rigidbody>().isKinematic = true;
        IsPlayerPickedGun = true;
    }

    public void DropGun()
    {
        Debug.Log("DropGun => " + currentGun.transform.gameObject.name);
        currentGun.transform.parent = null;
        currentGun.GetComponent<Rigidbody>().AddForce(playerCamera.transform.forward * 400f);
        currentGun.GetComponent<Rigidbody>().isKinematic = false;
        IsPlayerPickedGun = false;
        currentGun = null;
        isGun = false;
    }
}
