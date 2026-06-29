using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class HungrySoul : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public AudioSource audioSource;
    public AudioClip warningSound;
    public AudioClip pinnedLoopSound;
    public Animator animator;

    [Header("Girando")]
    public float minRadius = 2.5f;
    public float maxRadius = 3.5f;

    public float minAngularSpeed = 150f;
    public float maxAngularSpeed = 200f;

    [Header("Dash")]
    public float minDashCooldown = 3f;
    public float maxDashCooldown = 4f;

    public float dashSpeed = 18f;
    public float warningDuration = 0.15f;

    [Header("Cambio de Velocidad y Radio")]
    public float radiusChangeSpeed = 1f;
    public float speedChangeSpeed = 1f;

    private float angle;

    private float currentRadius;
    private float targetRadius;

    private float currentAngularSpeed;
    private float targetAngularSpeed;

    private bool dashing;

    private bool pinned = false;
    private bool canDamagePlayer = true;

    private void Start()
    {
        animator = GetComponent<Animator>();
        currentRadius = Random.Range(minRadius, maxRadius);
        targetRadius = currentRadius;

        currentAngularSpeed = Random.Range(minAngularSpeed, maxAngularSpeed);
        targetAngularSpeed = currentAngularSpeed;

        angle = Random.Range(0f, 360f);

        StartCoroutine(DashRoutine());
    }

    private void Update()
    {

        if (player == null || dashing || pinned)
            return;

        UpdateRadius();
        UpdateAngularSpeed();

        // Antihorario
        angle += currentAngularSpeed * Time.deltaTime;

        Vector2 offset =
            new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ) * currentRadius;

        transform.position = (Vector2)player.position + offset;
    }
    void UpdateRadius()
    {
        currentRadius = Mathf.Lerp(
            currentRadius,
            targetRadius,
            radiusChangeSpeed * Time.deltaTime
        );

        if (Mathf.Abs(currentRadius - targetRadius) < 0.05f)
        {
            targetRadius = Random.Range(minRadius, maxRadius);
        }
    }

    void UpdateAngularSpeed()
    {
        currentAngularSpeed = Mathf.Lerp(
            currentAngularSpeed,
            targetAngularSpeed,
            speedChangeSpeed * Time.deltaTime
        );

        if (Mathf.Abs(currentAngularSpeed - targetAngularSpeed) < 2f)
        {
            targetAngularSpeed =
                Random.Range(minAngularSpeed, maxAngularSpeed);
        }
    }

    public void PinSoul()
    {
        pinned = true;
        dashing = false;

        StopAllCoroutines();

        canDamagePlayer = false;

        animator.Play("Pinned");

        if (audioSource && pinnedLoopSound)
        {
            audioSource.Stop();
            audioSource.clip = pinnedLoopSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void ReleaseSoul()
    {
        StartCoroutine(ReleaseRoutine());
    }

    IEnumerator DashRoutine()
    {
        while (true)
        {
            if (pinned)
                continue;
            yield return new WaitForSeconds(
                Random.Range(minDashCooldown, maxDashCooldown)
            );

            if (player == null)
                continue;

            // Pausa previa
            if (audioSource && warningSound)
                audioSource.PlayOneShot(warningSound);

            yield return new WaitForSeconds(warningDuration);

            yield return StartCoroutine(PerformDash());
        }
    }

    IEnumerator PerformDash()
    {
        if (pinned)
            yield break;

        dashing = true;

        animator.Play("Dashing");

        Vector2 startPos = transform.position;

        Vector2 relative = startPos - (Vector2)player.position;

        // Punto opuesto del círculo
        Vector2 targetPos =
            (Vector2)player.position - relative;

        while (Vector2.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPos,
                dashSpeed * Time.deltaTime
            );

            yield return null;
        }

        // Recalcular ángulo para continuar girando alredeor del jugador
        Vector2 dir =
            (Vector2)transform.position - (Vector2)player.position;

        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        dashing = false;
    }

    IEnumerator ReleaseRoutine()
    {
        if (audioSource && audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = null;
        }

        yield return new WaitForSeconds(1f);

        pinned = false;
        canDamagePlayer = true;

        animator.Play("Orbit");
        StartCoroutine(DashRoutine());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerMovement player =
            collision.GetComponent<PlayerMovement>();

        if (player == null)
            return;

        if (pinned)
        {
            player.RecoverSoulBone();

            ReleaseSoul();
            return;
        }

        if (canDamagePlayer)
        {
            player.Die();
        }
    }
}