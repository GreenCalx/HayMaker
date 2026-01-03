using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Collections;


public class EndNail : MonoBehaviour
{
    public Rigidbody rb;
    public Transform NailTarget;
    [Header("Tweaks")]
    public float NailHitBonusForce = 1.5f;
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

            ContactPoint cp = collision.GetContact(0);
            float nailAim = Mathf.Abs(cp.point.y - transform.position.y);

            float bonusForce = Mathf.Lerp(NailHitBonusForce, 0f, nailAim);
            Vector3 bonusForceDir = bonusForce *  PC.rb.linearVelocity;
            bonusForceDir.y = 0f;
            bonusForceDir.z = 0f;
            rb.AddForce(bonusForceDir, ForceMode.Impulse);

            PC.OnGameFinish(nailAim);
            
            PostNailPlantCB.AddListener(
                () =>
                {
                    float nailDist = plankHit ? 0f : Vector3.Distance(NailTarget.transform.position, transform.position);
                    nailDist = Mathf.Clamp(nailDist, 0f, 999f);

                    float nailFactor = 1f - ((initDist - nailDist) / initDist);
                    
                    if (nailFactor <= 0f)
                        PC.OnVictory();
                    else
                        PC.OnLose();

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

    void OnCollisionStay(Collision collision)
    {
        if (collision.collider.transform.gameObject == NailTarget.gameObject)
        {
            plankHit = true;
        }
    }

    IEnumerator WaitNailPlant()
    {
        // wait for next frame
        yield return new WaitForSeconds(0.2f);

        while (rb.linearVelocity.magnitude > 0.1f)
        {
            if (plankHit)
                break;
            yield return null;
        }
        PostNailPlantCB.Invoke();
    }
}
