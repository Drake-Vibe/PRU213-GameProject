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
        // Smoothly interpolate position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
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
