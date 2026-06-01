using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class HungrySoul : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public AudioSource audioSource;
    public AudioClip warningSound;

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

    private void Start()
    {
        currentRadius = Random.Range(minRadius, maxRadius);
        targetRadius = currentRadius;

        currentAngularSpeed = Random.Range(minAngularSpeed, maxAngularSpeed);
        targetAngularSpeed = currentAngularSpeed;

        angle = Random.Range(0f, 360f);

        StartCoroutine(DashRoutine());
    }

    private void Update()
    {
        if (player == null || dashing)
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

    IEnumerator DashRoutine()
    {
        while (true)
        {
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
        dashing = true;

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
}