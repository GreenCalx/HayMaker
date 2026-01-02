using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float accelFreezeDuration = 1f;
    public float accelSpeedLoss = 0f;
    public Collider firstHitCollider;
    public Rigidbody rb;
    public float expulsionStrength = 1.5f;
    protected void OnFirstHit(Collider collider)
    {
        firstHitCollider.gameObject.SetActive(false);

        Vector3 expulseDir = transform.position - collider.transform.position;
        rb.AddForce(expulseDir * expulsionStrength, ForceMode.Impulse);
    }
}