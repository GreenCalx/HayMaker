using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        PlayerController PC = collision.collider.gameObject.GetComponentInParent<PlayerController>();
        if (PC!=null)
        {
            PC.Kill();
        }
    }

    void OnCollisionStay(Collision collision)
    {
        PlayerController PC = collision.collider.gameObject.GetComponentInParent<PlayerController>();
        if (PC!=null)
        {
            PC.Kill();
        }
    }
}
