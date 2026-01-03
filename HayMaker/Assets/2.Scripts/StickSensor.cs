using UnityEngine;

public class StickSensor : MonoBehaviour
{
    public PlayerController player;
    public LayerMask stepLayer;
    public LayerMask groundLayer;
    public Rigidbody rb;
    [Header("Animations")]
    public readonly string animParmBend = "IsBending";
    public Animator animator;
    public float bendTime;

    GameObject collidingObject;
    void OnCollisionEnter(Collision collision)
    {
        if (player.IsAirborne)
            return;
        if (player.isDead)
            return;

        collidingObject = collision.gameObject;
        if (((1 << collision.gameObject.layer) & stepLayer) != 0)
        {
            player.Bend();
            bendTime = 0f;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (player.IsAirborne)
            return;
        if (player.isDead)
            return;

        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            player.Drag();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (player.isDead)
            return;

        collidingObject = null;
        player.StickCollisionExit();
    }

    void Update()
    {
        if (player.isDead)
            return;

        animator.SetBool(animParmBend, player.IsBending);

    }

    void FixedUpdate()
    {
        if (player.IsBending)
            bendTime += Time.deltaTime;
    }
}
