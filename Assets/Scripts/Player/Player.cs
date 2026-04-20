using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private float wallSlideTime;
    [SerializeField] private float wallSlideTimeMax;

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

    Animator anim;
    AudioSource audio;

    public Transform spawnPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
        spawnPoint = GameObject.Find("SpawnPoint").transform;
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        anim.SetBool("IsMovement",true);

        // Detección de suelo
        grounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        // Coyote time
        if (grounded)
        {
            coyoteTimeCounter = coyoteTime;
            wallSlideTime = wallSlideTimeMax;
            canDash = true;
            canGrapple = true;
            anim.SetBool("IsJump", false); //"IsJump es el nombre de la variable "
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
            anim.SetBool("IsJump", true);
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
        bool isHoldingGrab = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton1);

        if (isTouchingWall && !grounded && wallSlideTime > 0 && isHoldingGrab)
        {
            isWallSliding = true;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                float pushDirection = facingRight ? -1f : 1f;
                rb.linearVelocity = new Vector2(pushDirection * speed, jumpForce);
                isWallSliding = false;
                wallSlideTime = 0;
                return;
            }
            // ----Subir o Bajar (Escalado)
            if (vertical > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallSlideSpeed);
            }
            else if (vertical < 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }
            else 
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed * 0);
            }
           wallSlideTime -= Time.deltaTime;
        }
        else
        {
            isWallSliding = false;
        }
        
        if (isDashing)
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

        if (Input.GetMouseButtonDown(1) && canGrapple)
        {
            StartGrapple();
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
        ToggleChildren(canGrapple);
    }

    private void FixedUpdate()
    {
        if (isDashing || isGrappleDashing) return;

        if (isGrappling)
        {
            Vector2 direction = (grapplePoint - (Vector2)transform.position).normalized;

            rb.linearVelocity = direction * grappleSpeed;

            anim.SetBool("IsDashing", true);
        }
        else
        {
            if (isWallSliding)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(horizontal * speed * Time.fixedDeltaTime, rb.linearVelocity.y);
            }
            anim.SetBool("IsDashing", false); //Ver que hace esto con el codigo 
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
        Vector2 rawDirection = (mousePos - (Vector2)transform.position).normalized;

        float angle = Mathf.Atan2(rawDirection.y, rawDirection.x) * Mathf.Rad2Deg;

        float snappedAngle = Mathf.Round(angle / 45f) * 45f;

        float rad = snappedAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

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

            rb.linearVelocity = direction * grappleDashForce;
        }
        canGrapple = false;
    }
    private void StopGrapple()
    {
        isGrappling = false;
        grappleline.enabled = false;
        rb.gravityScale = 1;
    }
    void ToggleChildren(bool state)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(state);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            transform.position = spawnPoint.position;
        }

        if (collision.gameObject.CompareTag("Estancia"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
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
            Destroy(collision.gameObject);
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