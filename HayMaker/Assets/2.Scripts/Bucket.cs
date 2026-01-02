using UnityEngine;

public class Bucket : MonoBehaviour
{
    public AudioSource bucketSFX;

    void OnCollisionEnter(Collision collision)
    {
        bucketSFX.Play();
    }
}
