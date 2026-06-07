using System.Collections;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("Teleport")]
    public Transform destination;

    [Header("Launch")]
    public float launchSpeed = 20f;

    public Vector2 launchDirection = Vector2.right;

    PlayerMovement player;

    private void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

        if (rb != null && destination != null && player.canTP)
        {
            StartCoroutine(TeleportPlayer(rb));
        }
    }

    private IEnumerator TeleportPlayer(Rigidbody2D rb)
    {
        player.canTP = false;

        if (player != null)
        {
            player.StopGrapple();
            player.StopDash();
        }

        rb.transform.position = new Vector3(2000f, 2000f, 0f);

        yield return new WaitForSeconds(1f);

        rb.transform.position = destination.position;

        rb.linearVelocity = Vector2.zero;

        rb.linearVelocity =
            launchDirection.normalized * launchSpeed;

        if (player != null)
        {
            player.externalLaunch = true;
            player.externalLaunchTime = 0.25f;
        }
    }
}