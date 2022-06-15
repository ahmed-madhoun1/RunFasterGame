using UnityEngine;

public class MoveCamera : MonoBehaviour
{

    [SerializeField]
    private Transform PlayerCameraPosition;

    void Update()
    {
        transform.position = PlayerCameraPosition.position;
    }
}