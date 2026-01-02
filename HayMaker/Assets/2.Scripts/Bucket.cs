using UnityEngine;

public class Bucket : Obstacle
{
    public AudioSource bucketSFX;

    void OnCollisionEnter(Collision collision)
    {
        bucketSFX.Play();
    }
}
