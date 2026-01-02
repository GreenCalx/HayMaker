using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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
    [Header("Stick Return")]
    public float zeroAngleEpsilon = 0.5f;      // degrees
    public float zeroAngularVelEpsilon = 0.05f;
    public float returnDamping = 1.2f;

    [Header("Audio")]
    public AudioSource dragSFX;
    public AudioSource bendSFX;
    public AudioSource nailHitSFX;

    [Header("Handles")]
    public AcceleratorAccumulator accumulator;
    public ParticleSystem DragPS;
    public ParticleSystem PerfectNailHitPS;
    public ParticleSystem GoodNailHitPS;
    public ParticleSystem BadNailHitPS;
    public Animator animator;
    public readonly string runningAnimParm = "IsRunning";
    public readonly string airborneAnimParm = "IsAirborne";

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 stickInput;
    private Vector2 prevStickInput;
    private float elapsedBendTime;

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
        if (IsBending)
        {
            elapsedBendTime += Time.deltaTime;
            if (elapsedBendTime > maxBendTime)
                Jump();
        }
        FSM.Refresh();
        UpdateRunningAnimationSpeed();
    }

    void FixedUpdate()
    {
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
        float bendTime = 1f + stickSensor.bendTime;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f);
        Vector3 jumpDir = (Vector3.up * jumpForce) + (Vector3.right * (jumpForce * 0.2f));
        jumpDir *= bendTime;
        rb.AddForce(jumpDir, ForceMode.Impulse);
    }

    public void Run(bool iState)
    {
        animator.SetBool(runningAnimParm, iState);
    }

    public void Drag()
    {
        IsDragging = true;
        animator.SetBool(runningAnimParm, true);
    }

    public void Bend()
    {
        if (IsBending)
            return;

        IsBending = true;
        elapsedBendTime = 0f;
        bendSFX.Play();
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
        IsAirborne = false;
        animator.SetBool(airborneAnimParm, false);
    }

    void OnCollisionExit(Collision collision)
    {
        IsAirborne = true;
        animator.SetBool(airborneAnimParm, true);
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
        SceneManager.LoadScene("Game",LoadSceneMode.Single);
    }

    public void OnGameFinish(float nailHitPrecision)
    {
        rb.linearVelocity = Vector3.zero;
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

    void UpdateRunningAnimationSpeed()
    {
        animator.speed = accumulator.current;
    }
}
