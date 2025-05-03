using Gabadie.GFSM;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Dependencies")]
    public PlayerController controller;
    public PlayerAnimator animator;
    public Cig cig;

    private void Start()
    {
        LockCursor();
    }

    private void Update()
    {
        InputFrame input = InputManager.Inst.lastInput;

        if (controller.fsm.Poll(Time.deltaTime))
        {
            //EventManager.Inst.Send(new PlayerTransitionEvent(this, controller.fsm.State));
        }

        if (cig.fsm.Poll(Time.deltaTime))
        {
            
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
