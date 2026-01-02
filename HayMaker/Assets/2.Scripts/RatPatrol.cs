using UnityEngine;
using UnityEngine.AI;

public class RatPatrol : MonoBehaviour
{
    public Transform point_A;
    public Transform point_B;

    public NavMeshAgent agent;
    bool gotoA = false;
    void Start()
    {
        if (gotoA)
            agent.SetDestination(point_A.transform.position);
        else
            agent.SetDestination(point_B.transform.position);
    }

    void Update()
    {
        if (agent.remainingDistance < 0.1f)
        {
            gotoA = !gotoA;
            if (gotoA)
                agent.SetDestination(point_A.transform.position);
            else
                agent.SetDestination(point_B.transform.position);
        }
    }
}
