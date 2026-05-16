using UnityEngine;

public class DashGhostController : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Animator anim;

    private bool previousCanDash;

    void Start()
    {
        if (player == null)
            player = GetComponentInParent<PlayerMovement>();

        if (anim == null)
            anim = GetComponent<Animator>();

        previousCanDash = player.CanDash();

        anim.SetBool("CanDash", previousCanDash);
    }

    void Update()
    {
        bool currentCanDash = player.CanDash();

        if (currentCanDash != previousCanDash)
        {
            anim.SetBool("CanDash", currentCanDash);

            previousCanDash = currentCanDash;
        }
    }
}