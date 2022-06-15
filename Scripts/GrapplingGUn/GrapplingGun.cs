using UnityEngine;

public class GrapplingGun : MonoBehaviour {
    
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, camera, player;
    public float maxDistance = 100f;
    private SpringJoint joint;
    public GunPick gunPick;
    public LayerMask whatIsGrapplingGun;
    public PlayerMovementAdvanced playerMovementAdvanced;
    public float moveSpeed = 20;

 
    void Update() {
        if (Input.GetKeyDown(KeyCode.F) && gunPick.IsPlayerPickedGun && gunPick.currentGun.tag == "GrapplingGun") {
            StartGrapple();
        }
        else if (Input.GetKeyUp(KeyCode.F) && gunPick.IsPlayerPickedGun && gunPick.currentGun.tag == "GrapplingGun") {
            StopGrapple();
        }
    }



    /// <summary>
    /// Call whenever we want to start a grapple
    /// </summary>
    void StartGrapple() {
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, maxDistance, whatIsGrappleable)) {
            playerMovementAdvanced.SetMoveSpeed(moveSpeed);
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = grapplePoint;

            float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

            //The distance grapple will try to keep from grapple point. 
            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;

            //Adjust these values to fit your game.
            joint.spring = 4.5f;
            joint.damper = 7f;
            joint.massScale = 4.5f;
        }
    }


    /// <summary>
    /// Call whenever we want to stop a grapple
    /// </summary>
    void StopGrapple() {
        Destroy(joint);
        playerMovementAdvanced.GetBackMoveSpeed();
    }



    public bool IsGrappling() {
        return joint != null;
    }

    public Vector3 GetGrapplePoint() {
        return grapplePoint;
    }
}
