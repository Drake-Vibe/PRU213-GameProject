using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("Target player transform to follow.")]
    public Transform target;
    [Tooltip("Smoothing factor (higher means faster follow).")]
    public float smoothSpeed = 8f;
    [Tooltip("Position offset of the camera relative to target.")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    private void Start()
    {
        if (target == null)
        {
            FindPlayerTarget();
        }

        Vector3 pos = transform.position;
        pos.z = offset.z;
        transform.position = pos;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindPlayerTarget();
            return;
        }

        // Target position with offset
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = offset.z; // Ensure camera stays at z = -10

        // Smoothly interpolate position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        smoothedPosition.z = offset.z;
        transform.position = smoothedPosition;
    }

    private void FindPlayerTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }
}
