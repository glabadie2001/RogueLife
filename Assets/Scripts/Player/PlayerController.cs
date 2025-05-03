using Sirenix.OdinInspector;
using Gabadie.GFSM;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Dependencies")]
    Rigidbody rb;

    [Required]
    [SerializeField]
    Camera cam;

    [Header("Movement")]
    [SerializeField]
    float speed = 5f;
    [SerializeField]
    float sprintMult = 1.5f;
    [SerializeField]
    float crouchMult = 0.5f;
    [SerializeField]
    float jumpForce = 350f;

    [Header("State Machine")]
    public FSM fsm = new FSM();

    Transition<State>[] transitions;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        fsm.AddState(new State("Idle"));

        fsm.AddState(new State("Walk",
            update: (float deltaTime) =>
            {
                Vector3 moveDirection = GetMovement(InputManager.Inst.lastInput.move);

                // Only modify the horizontal components of velocity
                Vector3 currentVelocity = rb.linearVelocity;
                Vector3 horizontalVelocity = new Vector3(moveDirection.x * speed, 0f, moveDirection.z * speed);
                rb.linearVelocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
            }
        ));

        fsm.AddState(new State("Sprint",
            update: (float deltaTime) =>
            {
                Vector3 moveDirection = GetMovement(InputManager.Inst.lastInput.move);

                // Only modify the horizontal components of velocity
                Vector3 currentVelocity = rb.linearVelocity;
                Vector3 horizontalVelocity = new Vector3(moveDirection.x * speed, 0f, moveDirection.z * speed * sprintMult);
                rb.linearVelocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
            }
        ));

        fsm.AddState(new State("Jump",
            onEnter: () =>
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        ));

        //TODO: Input bindings
        transitions = new Transition<State>[] {
            new Transition<State>(fsm["Idle"], fsm["Walk"], () =>
                InputManager.Inst.lastInput.move != Vector2.zero
                && !InputManager.Inst.lastInput.sprintHeld),

            new Transition<State>(fsm["Walk"], fsm["Idle"], () => InputManager.Inst.lastInput.move == Vector2.zero),

            new Transition<State>(fsm["Idle"], fsm["Sprint"], () =>
                InputManager.Inst.lastInput.move != Vector2.zero
                && InputManager.Inst.lastInput.sprintHeld),

            new Transition<State>(fsm["Sprint"], fsm["Idle"], () => InputManager.Inst.lastInput.move == Vector2.zero),

            new Transition<State>(fsm["Walk"], fsm["Sprint"], () => Input.GetKeyDown(KeyCode.LeftShift)),
            new Transition<State>(fsm["Sprint"], fsm["Walk"], () => Input.GetKeyUp(KeyCode.LeftShift)),

            //TODO: Jump transitions instantly cancel because the raycast instantly thinks we're grounded.
            //This works for now but I may want to make falling its own state?
            new Transition<State>(fsm["Idle"], fsm["Jump"], () => IsGrounded() && InputManager.Inst.lastInput.jumpDown),
            new Transition<State>(fsm["Jump"], fsm["Idle"], () => IsGrounded()),

            new Transition<State>(fsm["Walk"], fsm["Jump"], () => IsGrounded() && InputManager.Inst.lastInput.jumpDown),
            new Transition<State>(fsm["Jump"], fsm["Walk"], () => IsGrounded() && InputManager.Inst.lastInput.move != Vector2.zero),

            new Transition<State>(fsm["Sprint"], fsm["Jump"], () => IsGrounded() && InputManager.Inst.lastInput.jumpDown),
            new Transition<State>(fsm["Jump"], fsm["Sprint"], () => IsGrounded() && InputManager.Inst.lastInput.move != Vector2.zero && InputManager.Inst.lastInput.sprintHeld),
        };

        fsm.AddTransitions(transitions);

        fsm.Interrupt(fsm["Idle"]);
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + (Vector3.down * 1.1f));
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
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