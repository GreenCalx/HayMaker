using UnityEngine;
using UnityEngine.Events;

public class PlayerTriggerEvent : MonoBehaviour
{
    public UnityEvent EnterCB;
    public UnityEvent StayCB;
    public UnityEvent ExitCB;
    public bool EnterOnlyOnce = true;
    public bool StayOnlyOnce = true;
    public bool ExitOnlyOnce = true;
    bool enterTriggered = false;
    bool stayTriggered = false;
    bool exitTriggered = false;

    void OnTriggerEnter(Collider collider)
    {
        if (enterTriggered && EnterOnlyOnce)
            return;
        enterTriggered = true;
        EnterCB?.Invoke();
    }

    void OnTriggerStay(Collider collider)
    {
        if (stayTriggered && StayOnlyOnce)
            return;
        stayTriggered = true;
        StayCB?.Invoke();
    }

    void OnTriggerExit(Collider collider)
    {
        if (exitTriggered && ExitOnlyOnce)
            return;
        exitTriggered = false;
        ExitCB?.Invoke();
    }
}
