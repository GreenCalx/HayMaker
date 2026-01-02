using UnityEngine;

public class AcceleratorAccumulator : MonoBehaviour
{
    public float accumulationStep = 0.01f;
    public float min_acceleration = 1f;
    public float max_acceleration = 2.5f;
    public float current = 1f;

    [Header("Speedy VoiceLine")]
    public float accel_threshold_speedyvoice = 1.8f;
    public AudioSource speedySource;

    [Header("Woo VoiceLine")]
    public AudioSource maxSpeedSource;

    // internals
    float prevCurrent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        current = min_acceleration;
        prevCurrent = current;
    }

    public void Accumulate()
    {
        prevCurrent = current;

        current += accumulationStep;
        current = Mathf.Clamp(current, min_acceleration, max_acceleration);

        TryPlayVoiceLines();
            
    }

    public void Reset()
    {
        current = min_acceleration;
    }

    void TryPlayVoiceLines()
    {
        if (current >= max_acceleration)
        {
            maxSpeedSource.Play();
            return;
        }

        if ((current >= accel_threshold_speedyvoice)
            && (prevCurrent < accel_threshold_speedyvoice))
        {
            speedySource.Play();
            return;
        }
    }
}
