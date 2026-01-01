using UnityEngine;

public class StickSensor : MonoBehaviour
{
    public PlayerController player;
    public LayerMask stepLayer;
    public LayerMask groundLayer;

    GameObject collidingObject;
    void OnCollisionEnter(Collision collision)
    {
        collidingObject = collision.gameObject;
        if (((1 << collision.gameObject.layer) & stepLayer) != 0)
        {
            player.Bend();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject != collidingObject)
        {
            // ignore collision with different object
            return;
        }

         if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            player.Drag();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        collidingObject = null;
    }
}
