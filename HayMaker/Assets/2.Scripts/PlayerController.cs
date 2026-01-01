using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("View")]
    public PlayerFSM FSM;
    public bool IsDragging = false;
    public bool IsBending = false;
    public bool IsMoving => rb.linearVelocity.x > 0.1f;

    [Header("Movement")]
    public float baseSpeed = 3f;
    public float speedIncreaseRate = 0.2f;
    private float currentSpeed;

    [Header("Jump")]
    public float jumpForce = 6f;

    [Header("Stick")]
    public Transform stick;
    public float stickRotateSpeed = 120f;
    public float minStickAngle = -45f;
    public float maxStickAngle = 45f;

    [Header("Handles")]
    public MeshRenderer MR;

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector2 stickInput;
    private float speedTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentSpeed = baseSpeed;

        FSM = new PlayerFSM(this);
    }

    void Update()
    {
        speedTimer += Time.deltaTime;
        currentSpeed = baseSpeed + speedTimer * speedIncreaseRate;

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

        currentZ -= stickInput.y * stickRotateSpeed * Time.deltaTime;
        currentZ = Mathf.Clamp(currentZ, minStickAngle, maxStickAngle);

        stick.localRotation = Quaternion.Euler(0, 0, currentZ);
    }

    public void Jump()
    {
        Debug.Log("Player Jump");
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, 0f);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void Drag()
    {
        Debug.Log("Player Drag");
        IsDragging = true;
    }

    public void Bend()
    {
        Debug.Log("Player Bend");
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
}
