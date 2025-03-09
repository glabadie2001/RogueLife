using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField]
    Camera cam;

    [SerializeField]
    float speed = 5f;
    [SerializeField]
    float jumpForce = 350f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Prevent the rigidbody from rotating
    }

    void Update()
    {
        InputInfo input = ProcessInput();

        if (input.jump && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = GetMovement(ProcessInput().move);

        // Only modify the horizontal components of velocity
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity = new Vector3(moveDirection.x * speed, 0f, moveDirection.z * speed);
        rb.linearVelocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + (Vector3.down * 1.1f));
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    InputInfo ProcessInput()
    {
        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        // Normalize input if it exceeds 1 in combined length (for diagonal movement)
        if (movement.magnitude > 1f)
            movement.Normalize();

        return new InputInfo(movement, Input.GetKeyDown(KeyCode.Space));
    }

    Vector3 GetMovement(Vector2 input)
    {
        // Get forward and right vectors from the camera
        Vector3 forward = cam.transform.forward;
        Vector3 right = cam.transform.right;

        // Project vectors onto the horizontal plane
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Return direction in world space
        return forward * input.y + right * input.x;
    }
}

struct InputInfo
{
    public Vector2 move;
    public bool jump;

    public InputInfo(Vector2 _move, bool _jump)
    {
        move = _move;
        jump = _jump;
    }
}