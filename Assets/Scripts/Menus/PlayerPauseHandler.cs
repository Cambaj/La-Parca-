using UnityEngine;

public class PlayerPauseHandler : MonoBehaviour
{   
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;
    private PlayerMovement playerMovement;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
    }

    public void PausePlayer()
    {
        if (animator != null)
            animator.speed = 0f;

        if (audioSource != null)
            audioSource.Pause();

       if (playerMovement != null)
           playerMovement.enabled = false;
    }

    public void ResumePlayer()
    {
        if (animator != null)
            animator.speed = 1f;

        if (audioSource != null)
            audioSource.UnPause();

        if (playerMovement != null)
            playerMovement.enabled = true;
    }
    
}
