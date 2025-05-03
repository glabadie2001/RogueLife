using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField]
    Animator playerAnim;
    [SerializeField]
    Animator attackAnim;

    public void PlayAnim(string trigger)
    {
        playerAnim.Play(trigger);
    }

    public void SetFloat(string name, float value)
    {
        playerAnim.SetFloat(name, value);
    }
}
