using UnityEngine;

public class AnimationController : MonoBehaviour
{
   
    [Header("Animator Reference")]
    [SerializeField] private Animator animator;

    [Header("Parameter Names")]
    [SerializeField] private string Is_movingParameter = "Is_moving";
    [SerializeField] private string Is_idleParameter = "Is_idle";
    [SerializeField] private string Is_jumpingParameter = "Is_jumping";
    [SerializeField] private string Is_fallingParameter = "Is_falling";
    [SerializeField] private string Is_dashingParameter = "Is_dashing";


    private int Is_movingHash;
    private int Is_idleHash;
    private int Is_jumpingHash;
    private int Is_fallingHash;
    private int Is_dashingHash;


    private void Awake()
    {

        Is_movingHash = Animator.StringToHash(Is_movingParameter);
        Is_idleHash = Animator.StringToHash(Is_idleParameter);
        Is_jumpingHash = Animator.StringToHash(Is_jumpingParameter);
        Is_fallingHash = Animator.StringToHash(Is_fallingParameter);
        Is_dashingHash = Animator.StringToHash(Is_dashingParameter);

    }

    public void UpdateAnimation(bool Is_moving, bool Is_idle, bool Is_jumping, bool Is_falling, bool Is_dashing)
    {

        animator.SetBool(Is_movingHash, Is_moving);
        animator.SetBool(Is_idleHash, Is_idle);
        animator.SetBool(Is_jumpingHash, Is_jumping);
        animator.SetBool(Is_fallingHash, Is_falling);
        animator.SetBool(Is_dashingHash, Is_dashing);

    }
   
    public void PlayDeath()
    {
        if (animator != null)
        {
            animator.SetTrigger("IsDeath");
        }
    }

}

