using UnityEngine;

public class FollowPlayerCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = Vector3.zero;
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3 (player.position.x + offset.x, offset.y, offset.z); // Camera follows the player with specified offset position
    }
}
