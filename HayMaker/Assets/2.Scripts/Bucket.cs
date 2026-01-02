using UnityEngine;

public class Bucket : Obstacle
{
    public AudioSource bucketSFX;
    
    void OnCollisionEnter(Collision collision)
    {
        PlayerController PC = collision.collider.gameObject.GetComponentInParent<PlayerController>();
        StickSensor SS = collision.collider.gameObject.GetComponentInParent<StickSensor>();
        if ((PC==null)&&(SS==null))
            return;

        bucketSFX.Play();
        OnFirstHit(collision.collider);
    }
}
