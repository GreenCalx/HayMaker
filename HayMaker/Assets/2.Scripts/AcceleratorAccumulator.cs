using UnityEngine;

public class AcceleratorAccumulator : MonoBehaviour
{
    public float accumulationStep = 0.01f;
    public float min_acceleration = 1f;
    public float max_acceleration = 2.5f;
    public float current = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        current = min_acceleration;
    }

    public void Accumulate()
    {
        current += accumulationStep;
        current = Mathf.Clamp(current, min_acceleration, max_acceleration);
    }

    public void Reset()
    {
        current = min_acceleration;
    }
}
