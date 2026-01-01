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
    public float baseSpeed = 3f;
    public float speedIncreaseRate = 0.2f;
    public float stickDragStrength = 0.1f;
    private float currentSpeed;

    [Header("Jump")]
    public float jumpForce = 6f;

    [Header("Stick")]
    public Transform stick;
    public float stickRotateSpeed = 120f;
    public float minStickAngle = -45f;
    public float maxStickAngle = 45f;
    [Range(0f,1f)]
    public float returnToZeroAngleStrength = 0.5f;
    public float maxBendTime = 0f;

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

        RotateStick();

        FSM.Refresh();
    }

    void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveInput.x * currentSpeed;
        rb.linearVelocity = velocity;
    }

    void RotateStick()
    {
        if (!stick) 
            return;

        float currentZ = stick.localEulerAngles.z;
        if (currentZ > 180) currentZ -= 360;

        if (IsDragging)
        {
            float projectedZ = currentZ - stickInput.y * stickRotateSpeed * Time.deltaTime;
            projectedZ = Mathf.Clamp(projectedZ, currentZ, maxStickAngle);
            currentZ = projectedZ;
        } else
        {
            currentZ -= stickInput.y * stickRotateSpeed * Time.deltaTime;
            currentZ = Mathf.Clamp(currentZ, minStickAngle, maxStickAngle);
        }

        if (IsBending && stickInput.y == 0f)
            Jump();

        //apply counter motion to return to 0
        float counterAngle = returnToZeroAngleStrength * stickRotateSpeed * Time.deltaTime;
        if (currentZ > 0f)
        {
            currentZ -= counterAngle;
            currentZ = Mathf.Clamp(currentZ, 0f, currentZ);
        } else if ( currentZ < 0f)
        {
            if (stickInput.y > 0f)
                if (IsDragging && (prevStickInput.y <= stickInput.y))
                    counterAngle = 0f;

            currentZ += counterAngle;        
            currentZ = Mathf.Clamp(currentZ, currentZ, 0f);
        }

        // apply rotation
        stick.localRotation = Quaternion.Euler(0, 0, currentZ);

        prevStickInput = stickInput;
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
}
