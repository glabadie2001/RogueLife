using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Inst;

    public InputFrame lastInput;

    [SerializeField] bool jumpHeld = false;
    [SerializeField] bool crouchHeld = false;
    [SerializeField] bool sprintHeld = false;
    [SerializeField] bool aimHeld = false;
    [SerializeField] bool attackHeld = false;

    public string[] actions = {
        "Move",
        "Look",
        "Jump",
        "Attack",
        "Dash",
        "Crouch",
        "Slide",
        "Sprint",
        "Aim"
     };
    Dictionary<string, InputAction> actionMap = new Dictionary<string, InputAction>();

    private void Awake()
    {
        //Singleton boilerplate
        if (Inst == null)
            Inst = this;
        else if (Inst != this)
            Destroy(this);

        //Initialize mapping to global actions (see ProjectSettings/Input in engine)
        foreach (var action in actions)
        {
            actionMap.Add(action, InputSystem.actions.FindAction(action));
        }
    }

    private void Start()
    {
        lastInput = ProcessInput();
    }

    private void Update()
    {
        lastInput = ProcessInput();
    }

    public InputFrame ProcessInput()
    {
        Vector2 movement = actionMap["Move"].ReadValue<Vector2>();
        if (movement.magnitude > 1f)
            movement.Normalize();

        bool jumpDown = actionMap["Jump"].WasPressedThisFrame();
        bool jumpUp = actionMap["Jump"].WasReleasedThisFrame();
        jumpHeld = (jumpHeld || jumpDown) && !jumpUp;

        bool attackDown = actionMap["Attack"].WasPressedThisFrame();
        bool attackUp = actionMap["Attack"].WasReleasedThisFrame();
        attackHeld = (attackHeld || attackDown) && !attackUp;

        bool crouchDown = actionMap["Crouch"].WasPressedThisFrame();
        bool crouchUp = actionMap["Crouch"].WasReleasedThisFrame();
        crouchHeld = (crouchHeld || crouchDown) && !crouchUp;

        bool sprintDown = actionMap["Sprint"].WasPressedThisFrame();
        bool sprintUp = actionMap["Sprint"].WasReleasedThisFrame();
        sprintHeld = (sprintHeld || sprintDown) && !sprintUp;

        bool aimDown = actionMap["Aim"].WasPressedThisFrame();
        bool aimUp = actionMap["Aim"].WasReleasedThisFrame();
        aimHeld = (aimHeld || aimDown) && !aimUp;

        return new InputFrame(movement, jumpDown, jumpHeld, jumpUp, attackDown, attackHeld, crouchDown, crouchHeld, sprintHeld, aimHeld);
    }
}

[System.Serializable]
public struct InputFrame
{
    public Vector2 move;
    public bool jumpDown;
    public bool jumpHeld;
    public bool jumpUp;
    public bool attack;
    public bool attackHeld;
    public bool crouchDown;
    public bool crouchHeld;
    public bool sprintHeld;
    public bool aimHeld;

    public InputFrame(Vector2 _move, bool _jumpDown, bool _jumpHeld, bool _jumpUp, bool _attack, bool _attackHeld, bool _crouchDown, bool _crouchHeld, bool _sprintHeld, bool _aimHeld)
    {
        move = _move;
        jumpDown = _jumpDown;
        jumpHeld = _jumpHeld;
        jumpUp = _jumpUp;
        attack = _attack;
        attackHeld = _attackHeld;
        crouchDown = _crouchDown;
        crouchHeld = _crouchHeld;
        sprintHeld = _sprintHeld;
        aimHeld = _aimHeld;
    }
}
