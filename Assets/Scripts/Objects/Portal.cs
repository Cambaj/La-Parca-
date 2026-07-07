using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Teleport")]
    public Transform destination;

    [Header("Launch")]
    public float launchSpeed = 20f;

    public Vector2 launchDirection = Vector2.right;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip enterPortalSound;
    public AudioClip exitPortalSound;

    PlayerMovement player;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!canBeUsed) return;
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

        if (rb != null && destination != null && player.canTP)
        {
            AudioSource.PlayClipAtPoint(enterPortalSound, transform.position);
            StartCoroutine(TeleportPlayer(rb));
        }
    }

    private IEnumerator TeleportPlayer(Rigidbody2D rb)
    {
        canBeUsed = false;
        StartCoroutine(ResetCooldown());

        if (player != null)
        {
            player.StopGrapple();
            player.StopDash();
        }

        rb.transform.position = new Vector3(2000f, 2000f, 0f);
        yield return new WaitForSeconds(1f);

        rb.transform.position = destination.position;
        AudioSource.PlayClipAtPoint(exitPortalSound, destination.position);

        rb.linearVelocity = Vector2.zero;
        rb.linearVelocity = launchDirection.normalized * launchSpeed;

        if (player != null)
        {
            player.externalLaunch = true;
            player.externalLaunchTime = 0.25f;
        }
    }
    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(portalCooldown);
        canBeUsed = true;
    }
}