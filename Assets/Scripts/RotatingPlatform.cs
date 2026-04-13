using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Speed of rotation in degrees per second for each axis.")]
    public Vector3 rotationSpeed = new Vector3(0f, 50f, 0f);

    [Tooltip("Whether to rotate relative to its own local space or the world space.")]
    public Space rotationSpace = Space.Self;

    public Quaternion CurrentRotationDelta { get; private set; } = Quaternion.identity;

    void Update()
    {
        Quaternion initialRot = transform.rotation;

        // Continuously rotate the platform at the specified speed
        transform.Rotate(rotationSpeed * Time.deltaTime, rotationSpace);

        CurrentRotationDelta = transform.rotation * Quaternion.Inverse(initialRot);
    }
}
