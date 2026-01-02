using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Collections;


public class EndNail : MonoBehaviour
{
    public Rigidbody rb;
    public Transform NailTarget;
    [Header("GameOver UI")]
    public UIGameOver uiGameOver;

    [Header("Internal View")]
    public UnityEvent PostNailPlantCB;
    bool plankHit = false;

    float initDist = 0f;
    void Start()
    {
        initDist = Vector3.Distance(NailTarget.transform.position, transform.position);
    }

    void OnCollisionEnter(Collision collision)
    {
        StickSensor SS = collision.collider.gameObject.GetComponentInParent<StickSensor>();
        if (SS!=null)
        {
            PlayerController PC = SS.player;
            PC.OnGameFinish();
            PostNailPlantCB.AddListener(
                () =>
                {
                    float nailFactor = plankHit ? 0f : Vector3.Distance(NailTarget.transform.position, transform.position);
                    nailFactor = Mathf.Clamp(nailFactor, 0f, initDist);
                    
                    uiGameOver.gameObject.SetActive(true);
                    uiGameOver.Setup(nailFactor);
                    // Continue game here / stop time
                }
            );
            StartCoroutine(WaitNailPlant());
        }

        if (collision.collider.transform.gameObject == NailTarget.gameObject)
        {
            plankHit = true;
        }
    }

    IEnumerator WaitNailPlant()
    {
        while (rb.linearVelocity.magnitude > 0.1f)
        {
            if (plankHit)
                break;
            yield return null;
        }
        PostNailPlantCB.Invoke();
    }
}
