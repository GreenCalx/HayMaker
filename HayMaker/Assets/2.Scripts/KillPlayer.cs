using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    public float repulseForce = 0f;
    bool triggered = false;

    void OnCollisionEnter(Collision collision)
    {
        if (triggered)
            return;

        PlayerController PC = collision.collider.gameObject.GetComponentInParent<PlayerController>();
        if (PC!=null)
        {
            PC.Kill();
            triggered = true;
            Vector3 dir = collision.collider.transform.position - transform.position;
            PC.rb.AddForce(dir * repulseForce, ForceMode.Impulse);
            Destroy(this);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (triggered)
            return;

        PlayerController PC = collision.collider.gameObject.GetComponentInParent<PlayerController>();
        if (PC!=null)
        {
            PC.Kill();
            triggered = true;
            Vector3 dir = collision.collider.transform.position - transform.position;
            PC.rb.AddForce(dir * repulseForce, ForceMode.Impulse);
            Destroy(this);
        }
    }
}
