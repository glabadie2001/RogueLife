using Gabadie.GFSM;
using UnityEngine;

public class Cig : MonoBehaviour
{
    [SerializeField] Animator anim;

    public FSM<State> fsm = new FSM<State>();
    public GameObject smokeTemplate;
    public Transform tip;
    ParticleSystem smoke;

    [SerializeField] float blend = 0;
    float moveRate = 4f;
    bool smoked = false;

    private void Start()
    {
        fsm.AddState(new State("Idle"));

        fsm.AddState(new State("Smoke",
            onEnter: () =>
            {
                anim.SetBool("Smoke", true);
                smoked = false;
            },
            update: (float dT) =>
            {
                blend = Mathf.MoveTowards(blend, 1, moveRate * dT);
                if (!smoked && blend == 1 && InputManager.Inst.lastInput.attackHeld)
                {
                    smoked = true;
                    smoke = Instantiate(smokeTemplate, tip).GetComponent<ParticleSystem>();
                }
                if (smoked && !InputManager.Inst.lastInput.attackHeld)
                {
                    smoke.Stop();
                    smoked = false;
                }
            },
            onExit: (State s) =>
            {
                anim.SetBool("Smoke", false);
                if (smoke != null)
                {
                    smoke.Stop();
                    smoke = null;
                }
            }
        ));

        fsm.AddState(new State("End",
            update: (float dT) =>
            {
                blend = Mathf.MoveTowards(blend, 0, moveRate * dT);
            }
        ));

        fsm.AddTransition(new Transition<State>(fsm["Idle"], fsm["Smoke"], () => InputManager.Inst.lastInput.aimHeld));
        fsm.AddTransition(new Transition<State>(fsm["Smoke"], fsm["End"], () => !InputManager.Inst.lastInput.aimHeld));
        fsm.AddTransition(new Transition<State>(fsm["End"], fsm["Idle"], () => blend <= 0));


        fsm.Interrupt(fsm["Idle"]);
    }

    private void Update()
    {
        anim.SetFloat("Blend", blend);
    }
}
