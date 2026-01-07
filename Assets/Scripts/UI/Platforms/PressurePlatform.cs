using UnityEngine;

public class PressurePlatform : MonoBehaviour
{
    [Header("Platform Movement")]
    public Transform platform;
    public Transform platformPointA;
    public Transform platformPointB;

    [Header("Object X Movement")]
    public Transform objectX;
    public Transform objectXPointA;
    public Transform objectXPointB;

    [Header("Settings")]
    public float moveSpeed = 2f;

    private int objectsOnPlatform = 0;

    void Start()
    {
        platform.position = platformPointA.position;
        objectX.position = objectXPointA.position;
    }

    void OnCollisionEnter(Collision collision)
    {
        objectsOnPlatform++;
    }

    void OnCollisionExit(Collision collision)
    {
        objectsOnPlatform = Mathf.Max(0, objectsOnPlatform - 1);
    }

    void Update()
    {
        bool pressed = objectsOnPlatform > 0;

        // Platform movement
        platform.position = Vector3.MoveTowards(
            platform.position,
            pressed ? platformPointB.position : platformPointA.position,
            moveSpeed * Time.deltaTime
        );

        // Object X movement
        objectX.position = Vector3.MoveTowards(
            objectX.position,
            pressed ? objectXPointB.position : objectXPointA.position,
            moveSpeed * Time.deltaTime
        );
    }
}
