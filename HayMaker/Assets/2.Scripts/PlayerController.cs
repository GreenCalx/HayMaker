using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
public class PlayerController : MonoBehaviour
{
    [Header("View")]
    public PlayerFSM FSM;
    public bool IsDragging = false;
    public bool IsBending = false;
    public bool IsAirborne = false;
    public bool IsMoving => Mathf.Abs(rb.linearVelocity.x) > 0.1f;

    [Header("Movement")]
    public AnimationCurve SpeedOverTime;
    public float baseSpeed = 3f;
    public float stickDragStrength = 0.1f;
    private float currentSpeed;

    [Header("Jump")]
    public float jumpForce = 6f;

    [Header("Stick")]
    public StickSensor stickSensor;
    public Transform stick;
    public float maxBendTime = 0f;
    [Header("Stick Physics")]
    public Rigidbody stickRb;
    public float stickTorque = 15f;
    public float returnTorque = 5f;
    public float maxAngularVelocity = 10f;

    [Header("Obstacles")]
    public LayerMask obstacleLayer;
    public LayerMask groundLayer;

    [Header("Audio")]
    public AudioSource dragSFX;
    public AudioSource bendSFX;
    public AudioSource nailHitSFX;
    public AudioSource victorySFX;
    public AudioSource loseSFX;
    public AudioSource deathFX;
    public GameObject prefab_footstepSFX;
    public List<AudioClip> footstepClips;
    public Transform handle_footstepBuffer;

    [Header("UI")]
    public UIGameOver uiGameOver;

    [Header("Handles")]
    public SkinnedMeshRenderer selfMR;
    public AcceleratorAccumulator accumulator;
    public ParticleSystem DragPS;
    public ParticleSystem PerfectNailHitPS;
    public ParticleSystem GoodNailHitPS;
    public ParticleSystem BadNailHitPS;
    public ParticleSystem jumpPS;
    public Animator animator;
    public readonly string runningAnimParm = "IsRunning";
    public readonly string airborneAnimParm = "IsAirborne";
    public readonly string victoryAnimTrigg = "OnWin";
    public readonly string failAnimTrigg = "OnLose";
    public readonly string deadAnimTrigg = "Dead";

    public Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 stickInput;
    private Vector2 prevStickInput;
    private float elapsedBendTime;
    bool firstFootstep = false;
    bool secondFootstep = false;
    public bool isDead = false;

    private bool freezeMovements = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = baseSpeed;
        prevStickInput = Vector2.zero;
        FSM = new PlayerFSM(this);
        freezeMovements = false;
    }

    void Update()
    {
        if (isDead)
            return;

        if (IsBending)
        {
            elapsedBendTime += Time.deltaTime;
            if (elapsedBendTime > maxBendTime)
                Jump();
        }
        if (FSM!=null)
            FSM.Refresh();

        UpdateRunningAnimationSpeed();
    }

    void FixedUpdate()
    {
        if (isDead)
            return;
        Move();
        RotateStick();
    }

    void Move()
    {
        if (freezeMovements)
            return;

        Vector3 velocity = rb.linearVelocity;

        // only forward
        float inputx = Mathf.Clamp(moveInput.x, 0f, 1f);
        if (inputx > 0f)
            accumulator.Accumulate();
        else
            accumulator.Reset();

        velocity.x = inputx * currentSpeed * accumulator.current;
        if (IsDragging)
        {
            velocity.x -= velocity.x * stickDragStrength;
        }

        rb.linearVelocity = velocity;
    }

    void RotateStick()
    {
        if (freezeMovements)
            return;

        if (stickInput.y == 0f)
        {
            ApplyCounterTorque();
            return;
        }

        // Prevent insane spinning
        stickRb.maxAngularVelocity = maxAngularVelocity;

        float input = stickInput.y;

        // INPUT TORQUE
        float torque = -input * stickTorque;

        stickRb.AddTorque(Vector3.forward * torque, ForceMode.Acceleration);

        // RETURN TO ZERO (SPRING)
        if (Mathf.Abs(input) < 0.05f)
        {
            ApplyCounterTorque();
        }

        // BENDING RELEASE /JUMP
        if (IsBending && (stickInput.y<0.1f))
        {
            Jump();
        }

        prevStickInput = stickInput;
    }

    void ApplyCounterTorque()
    {
        float angle = GetStickAngle();
        float angularVel = stickRb.angularVelocity.z;
        // Apply return torque toward 0
        float springTorque = -Mathf.Sign(angle) * returnTorque;
        stickRb.AddTorque(Vector3.forward * springTorque,
                        ForceMode.Acceleration);
        // If we've crossed or reached zero → SNAP
        if (Mathf.Sign(angle) != Mathf.Sign(angle - angularVel * Time.fixedDeltaTime))
        {
            stickRb.angularVelocity = Vector3.zero;
            stickRb.MoveRotation(Quaternion.identity);
        }
    }

    public void Jump()
    {
        if (isDead)
            return;

        float bendTime = 1f + stickSensor.bendTime;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f);
        Vector3 jumpDir = (Vector3.up * jumpForce) + (Vector3.right * (jumpForce * 0.25f));
        jumpDir *= bendTime;
        rb.AddForce(jumpDir, ForceMode.Impulse);
        jumpPS.Play();
        bendSFX.Play();
    }

    public void Run(bool iState)
    {
        if (isDead)
            return;
        animator.SetBool(runningAnimParm, iState);
    }

    public void Drag()
    {
        if (isDead)
            return;

        IsDragging = true;
        animator.SetBool(runningAnimParm, true);
    }

    public void Bend()
    {
        if (IsBending ||isDead)
            return;

        IsBending = true;
        elapsedBendTime = 0f;
    }

    public void StickCollisionExit()
    {
        IsDragging = false;
        IsBending = false;
    }

    // Input System callbacks
    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnStickRotate(InputAction.CallbackContext ctx)
    {
        stickInput = ctx.ReadValue<Vector2>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead)
            return;

        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            Obstacle obstacle = collision.gameObject.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                accumulator.Freeze(obstacle.accelFreezeDuration, obstacle.accelSpeedLoss);
            }
            return;
        }

        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            IsAirborne = false;
            animator.SetBool(airborneAnimParm, false);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (isDead)
            return;

        if (((1 << collision.gameObject.layer) & obstacleLayer) != 0)
        {
            return;
        }
        
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            IsAirborne = true;
            animator.SetBool(airborneAnimParm, true);
        }
    }

    float GetStickAngle()
    {
        float angle = stickRb.transform.localEulerAngles.z;
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }

    public void ResetStage(InputAction.CallbackContext ctx)
    {
        Reset();
    }

    void Reset()
    {
        SceneManager.LoadScene("Game",LoadSceneMode.Single);
    }

    public void OnGameFinish(float nailHitPrecision)
    {
        //rb.linearVelocity = Vector3.zero;
        freezeMovements = true;

        Debug.Log("Nail aim : " + nailHitPrecision);
        if (nailHitPrecision < 0.1f)
            PerfectNailHitPS.Play();
        else if (nailHitPrecision < 0.5f)
            GoodNailHitPS.Play();
        else
            BadNailHitPS.Play();

        nailHitSFX.Play();

        // Force run animation during finis..
        Run(true);
    }

    public void OnVictory()
    {
        animator.SetTrigger(victoryAnimTrigg);
        victorySFX.Play();
    }

    public void OnLose()
    {
        animator.SetTrigger(failAnimTrigg);
        loseSFX.Play();
    }

    void UpdateRunningAnimationSpeed()
    {
        if (isDead)
            return;
            
        animator.speed = accumulator.current;
        
        // play footsteps in a dirty place
        AnimatorStateInfo asi = animator.GetCurrentAnimatorStateInfo(0);
        if (asi.IsName("RUNNING"))
        {
            float animPlaybackElapsed = asi.normalizedTime % 1;
            if (animPlaybackElapsed < 0.05f)
            {
                firstFootstep = false;
                secondFootstep = false;
            }
            else if ((animPlaybackElapsed > 0.2f) && (!firstFootstep))
            {
                firstFootstep = true;
                FootstepSFXPlay();
            }
            else if ((animPlaybackElapsed > 0.8f) && (!secondFootstep))
            {
                secondFootstep = true;
                FootstepSFXPlay();
            }
        }
    }

    public void FootstepSFXPlay()
    {
        GameObject new_source = GameObject.Instantiate(prefab_footstepSFX);
        new_source.transform.parent = handle_footstepBuffer;

        AudioSource as_audio = new_source.GetComponent<AudioSource>();
        as_audio.clip = footstepClips[UnityEngine.Random.Range(0, footstepClips.Count)];
        as_audio.Play();
        GameObject.Destroy(new_source, 1f);
    }

    public void Kill()
    {
        if (isDead)
            return;

        isDead = true;
        IsAirborne  = false;
        IsDragging  = false;
        IsBending   = false;

        FSM.Kill();
        FSM = null;

        rb.linearVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        stickSensor.rb.constraints = RigidbodyConstraints.FreezeAll;



        deathFX.Play();
        // Animate Death here
        stickSensor.rb.isKinematic = true;
        rb.isKinematic = true;

        animator.speed = 1f;
        animator.SetTrigger(deadAnimTrigg);

        // Do stuff like animate ..
        uiGameOver.Setup(-1f);
        uiGameOver.gameObject.SetActive(true);
        
        //Reset();
    }
}
