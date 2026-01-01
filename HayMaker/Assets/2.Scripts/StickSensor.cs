using UnityEngine;

public class StickSensor : MonoBehaviour
{
    public PlayerController player;
    public LayerMask stepLayer;
    public LayerMask groundLayer;

    GameObject collidingObject;
    void OnCollisionEnter(Collision collision)
    {
        if (player.IsAirborne)
            return;

        collidingObject = collision.gameObject;
        if (((1 << collision.gameObject.layer) & stepLayer) != 0)
        {
            player.Bend();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (player.IsAirborne)
            return;

        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            player.Drag();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        collidingObject = null;
        player.StickCollisionExit();
    }
}
