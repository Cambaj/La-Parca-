using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;

    [Header("Detección de suelo")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;

    [Header("Salto variable")]
    [SerializeField] private float lowJumpMultiplier;
    [SerializeField] private float fallMultiplier;

    [Header("Jump Buffer & Coyote Time")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.15f;

    [Header("Grapping Hook")]
    [SerializeField] private float grappleMaxDistance = 10f;
    [SerializeField] private float grappleSpeed = 20f;
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private LineRenderer grappleline;
   
    private Vector2 grapplePoint;
    private bool isGrappling = false;

    private float jumpBufferCounter;
    private float coyoteTimeCounter;

    private float horizontal;
    private float vertical;
    private bool grounded;
    private Rigidbody2D rb;

    private bool facingRight = true;

    [Header("Wall Slide")]
    [SerializeField] private float wallSlideSpeed = 10f;
    private bool isTouchingWall;
    private bool isWallSliding;
    [SerializeField] private float wallSlideTime = 3f;

    [Header("Dash")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    private bool canDash = true;
    private bool isDashing = false;
    private float dashTime;

    [Header("Grapple Dash")]
    [SerializeField] private float grappleDashForce = 18f;
    [SerializeField] private float grappleDashDuration = 0.15f;

    private bool canGrapple = true;
    private bool isGrappleDashing = false;
    private float grappleDashTime;


    [Header("Audio")]
    [SerializeField] private AudioClip JumpSound;

    //Animator anim;
    AudioSource audio;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        // Detección de suelo
        grounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        // Coyote time
        if (grounded)
        {
            coyoteTimeCounter = coyoteTime;
            wallSlideTime = 3f;
            canDash = true;
            canGrapple = true;
            //anim.SetBool("IsJump", false);
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            //anim.SetBool("IsJump", true);
        }
        // Jump buffer
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // ---- SALTO NORMAL ----
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && Time.timeScale == 1f)
        {
            DoJump(Vector2.up);
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            audio.PlayOneShot(JumpSound);
        }

        // ---- WALL SLIDE ----
        if (isTouchingWall && !grounded && rb.linearVelocity.y < 0 && wallSlideTime > 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);

            if (Input.GetKeyDown(KeyCode.Space) || vertical > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, +wallSlideSpeed);
            }

            if (vertical < 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed * 2);
            }
            wallSlideTime -= Time.deltaTime;
        }
        else
        {
            isWallSliding = false;
        }

        // ---- DASH ----
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !grounded)
        {
            canDash = false;
            isDashing = true;
            dashTime = dashDuration;

            Vector2 dashDirection = new Vector2(horizontal, vertical);

            if (dashDirection == Vector2.zero)
            {
                dashDirection = facingRight ? Vector2.right : Vector2.left;
            }

            dashDirection.Normalize();

            rb.linearVelocity = dashDirection * dashForce;
        }
        if (isDashing)
        {
            rb.gravityScale = 0;
            dashTime -= Time.deltaTime;

            if (dashTime <= 0)
            {
                isDashing = false;
                rb.gravityScale = 1;
            }
        }
        if (isGrappleDashing)
        {
            grappleDashTime -= Time.deltaTime;

            if (grappleDashTime <= 0)
            {
                isGrappleDashing = false;
                rb.gravityScale = 1;
            }
        }

        // ---- SALTO VARIABLE ----
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // ---- FLIP DEL PLAYER ----
        if (horizontal < 0 && !facingRight && Time.timeScale != 0)
        {
            Flip();
        }
        else if (horizontal > 0 && facingRight && Time.timeScale != 0)
        {
            Flip();
        }

        // ---- GUADAÑA ----

        if (Input.GetMouseButtonDown(1) && canGrapple && !grounded)
        {
            StartGrapple();
            canGrapple = false;
        }

        if (Input.GetMouseButtonUp(1))
            {
                StopGrapple();
            }
        if (isGrappling)
        {
            // Forzamos el dibujo de la línea convirtiendo a Vector3
            grappleline.SetPosition(0, new Vector3(transform.position.x, transform.position.y, 0));
            grappleline.SetPosition(1, new Vector3(grapplePoint.x, grapplePoint.y, 0));

            // IMPORTANTE: Cambiado de > a < 
            // Si la distancia es menor a 0.5 unidades, soltamos el gancho
            if (Vector2.Distance(transform.position, grapplePoint) < 0.5f)
            {
                StopGrapple();
            }
        }

    }

    private void FixedUpdate()
    {
        if (isDashing || isGrappleDashing) return;

        if (isGrappling)
        {
            Vector2 direction = (grapplePoint - (Vector2)transform.position).normalized;

            rb.linearVelocity = direction * grappleSpeed;
        }
        else 
        { 
            rb.linearVelocity = new Vector2(horizontal * speed * Time.fixedDeltaTime, rb.linearVelocity.y);
        }
        

    }

    private void Flip()
    {
        facingRight = !facingRight;

        // Volteamos al Player
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void DoJump(Vector2 direction)
    {
        rb.AddForce(direction * jumpForce, ForceMode2D.Impulse);
        //anim.SetBool("IsJump", true);
    }

    private void StartGrapple()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, grappleMaxDistance, grappleLayer);

        if (hit.collider != null)
        {
            grapplePoint = hit.point;
            isGrappling = true;
            grappleline.enabled = true;
        }
        else
        {
            isGrappleDashing = true;
            grappleDashTime = grappleDashDuration;

            rb.gravityScale = 0;
            rb.linearVelocity = direction * grappleDashForce;
        }
    }

    private void StopGrapple()
    {
        isGrappling = false;
        grappleline.enabled = false;
        rb.gravityScale = 1;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Si el contacto es lateral (pared)
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    isTouchingWall = true;
                    canDash = true;
                    return;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Entity"))
        {
            canDash = true;
            canGrapple = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            isTouchingWall = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}