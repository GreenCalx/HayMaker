using UnityEngine;

public class FollowPlayerCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = Vector3.zero;

    [Header("Y Follow")]
    public float ySmoothSpeed = 5f;

    void LateUpdate()
    {
        float targetY = player.position.y + offset.y;

        float smoothY = Mathf.Lerp(
            transform.position.y,
            targetY,
            ySmoothSpeed * Time.deltaTime
        );

        transform.position = new Vector3(
            player.position.x + offset.x,
            smoothY,
            offset.z
        );
    }
}
