using UnityEngine;
using Mirror;

namespace UpWeGo
{
public class CameraFollow : NetworkBehaviour
{
    public Vector3 offset = new Vector3(0, 5, -10);
    private Transform playerTransform;
    private Camera cam;

    void Start()
    {
        if (!isLocalPlayer)
        {
            enabled = false;
            return;
        }
        cam = Camera.main;
        playerTransform = transform;
    }

    void LateUpdate()
    {
        if (!isLocalPlayer || cam == null)
            return;
        cam.transform.position = playerTransform.position + offset;
        cam.transform.LookAt(playerTransform);
    }
}
}