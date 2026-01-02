using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    public float repulseForce = 0f;
    void OnCollisionEnter(Collision collision)
    {
        PlayerController PC = collision.collider.gameObject.GetComponentInParent<PlayerController>();
        if (PC!=null)
        {
            PC.Kill();
            Vector3 dir = transform.position - collision.collider.transform.position;
            PC.rb.AddForce(dir * repulseForce, ForceMode.Impulse);
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
