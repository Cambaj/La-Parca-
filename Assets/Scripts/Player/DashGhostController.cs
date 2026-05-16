using UnityEngine;

public class DashGhostController : MonoBehaviour
{
    [Header("Poner al Player para saber el estado del Dash")]
    [SerializeField] private PlayerMovement player;
    [Header("Animator de DashGhost")]
    [SerializeField] private Animator anim;

    private bool previousCanDash;

    void Start()
    {
        if (player == null)
            player = GetComponentInParent<PlayerMovement>();

        if (anim == null)
            anim = GetComponent<Animator>();

        previousCanDash = player.CanDash();

        if (previousCanDash)
        {
            anim.Play("DashReady");
        }
        else
        {
            anim.Play("DashOff");
        }
    }

    void Update()
    {
        bool currentCanDash = player.CanDash();

        if (currentCanDash != previousCanDash)
        {
            if (currentCanDash)
            {
                anim.Play("DashTurnOn");
            }
            else
            {
                anim.Play("DashTurnOff");
            }

            previousCanDash = currentCanDash;
        }
    }
    public void SetReadyState()
    {
        anim.Play("DashReady");
    }


    public void SetOffState()
    {
        anim.Play("DashOff");
    }
}