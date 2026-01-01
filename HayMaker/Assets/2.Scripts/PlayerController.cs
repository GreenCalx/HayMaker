using UnityEngine;
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
    public float speedIncreaseRate = 0.2f;
    public float stickDragStrength = 0.1f;
    private float currentSpeed;

    [Header("Jump")]
    public float jumpForce = 6f;

    [Header("Stick")]
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


    [Header("Handles")]
    public MeshRenderer MR;
    public ParticleSystem DragPS;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 stickInput;
    private Vector2 prevStickInput;
    private float speedTimer;
    private float elapsedBendTime;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = baseSpeed;
        prevStickInput = Vector2.zero;
        FSM = new PlayerFSM(this);
    }

    void Update()
    {
        speedTimer += Time.deltaTime;

        currentSpeed = baseSpeed + speedTimer * speedIncreaseRate;
        
        if (IsDragging)
            currentSpeed -= currentSpeed * stickDragStrength;

        if (IsBending)
        {
            elapsedBendTime += Time.deltaTime;
            if (elapsedBendTime > maxBendTime)
                Jump();
        }

        FSM.Refresh();
    }

    void FixedUpdate()
    {
        Move();
        RotateStick();
    }

    void Move()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveInput.x * currentSpeed;
        rb.linearVelocity = velocity;
    }

    void RotateStick()
    {
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
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f);
        Vector3 jumpDir = (Vector3.up * jumpForce) + (Vector3.right * (jumpForce * 0.1f));
        rb.AddForce(jumpDir, ForceMode.Impulse);
    }

    public void Drag()
    {
        IsDragging = true;
    }

    public void Bend()
    {
        if (IsBending)
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
        IsAirborne = false;
    }

    void OnCollisionExit(Collision collision)
    {
        IsAirborne = true;
    }

    float GetStickAngle()
    {
        float angle = stickRb.transform.localEulerAngles.z;
        if (angle > 180f)
            angle -= 360f;
        return angle;
    }

}
