using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxFallVelocity;

    [Header("Detección de suelo")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask groundLayer;

    [Header("Salto variable")]
    [SerializeField] private float lowJumpMultiplier;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float peakHoverThreshold = 2f;
    [SerializeField] private float peakHoverGravity = 0.2f;
    [SerializeField] private float normalGravityScale = 1f;
    [Header("Jump Buffer & Coyote Time")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.15f;

    [Header("Grapping Hook")]
    [SerializeField] private float grappleMaxDistance = 10f;
    [SerializeField] private float grappleSpeed = 20f;
    [SerializeField] private float grappleDuration = 3f;
    private float grappleTimer;
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private LineRenderer grappleline;
    [SerializeField] private GameObject grappleObject;

    private Vector2 grapplePoint;
    private bool isGrappling = false;

    private float jumpBufferCounter;
    private float coyoteTimeCounter;

    private float horizontal;
    private float vertical;
    //Mando Solo
    private float horizontalRightStick;
    private float verticalRightStick;
    private bool grounded;
    private Rigidbody2D rb;

    private bool facingRight = true;

    private Vector2 wallNormal;

    [Header("Wall Slide")]
    [SerializeField] private float wallSlideSpeed = 10f;
    private bool isTouchingWall;
    private bool isWallSliding;
    private float wallSlideTime;
    [SerializeField] private float wallSlideTimeMax;
    private bool isHoldingGrab;
    [Header("Wall Jump")]
    [SerializeField] private float wallJumpForceX = 20f;
    [SerializeField] private float wallJumpForceY = 10f;
    private bool isWallJumping;
    private float wallJumpTimer;
    [SerializeField] private float wallJumpDuration = 0.2f;

    [Header("Dash")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    private Vector2 dashVelocity;
    private bool canDash = true;
    private bool isDashing = false;
    private float dashTime;
    private bool canGrapple = true;
    private bool hasJumped = false;
    
    //No borrar estas variables, se van a usar despues 
    [Header("GrappleCorector")]
    [SerializeField] private bool TopRight;
    [SerializeField] private bool TopLeft;
    [SerializeField] private bool BottomRight;
    [SerializeField] private bool BottomLeft;

    //Portal
    [HideInInspector] public bool externalLaunch;
    [HideInInspector] public float externalLaunchTime;

    [Header("Audio")]
    AudioSource audio;
    [SerializeField] private AudioSource walkAudio;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip grappleLaunchSound;
    [SerializeField] private AudioClip grappleHookSound;
    [SerializeField] private AudioClip grappleRecoverSound;
    [SerializeField] private AudioClip dashSound;
    [SerializeField] private AudioClip walkGrassSound;

    private bool isDead = false;

    private Vector2 controllerAim;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    Animator anim;
    
    //public Transform spawnPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        //spawnPoint = GameObject.Find("SpawnPoint").transform;
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        horizontalRightStick = Input.GetAxisRaw("RightStickHorizontal");
        verticalRightStick = Input.GetAxisRaw("RightStickVertical");

        controllerAim = new Vector2(horizontalRightStick, verticalRightStick);

        if (controllerAim.magnitude < 0.2f)
        {
            controllerAim = Vector2.zero;
        }
        else
        {
            controllerAim.Normalize();
        }


        // Ground Detection
        grounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        // Coyote time
        if (grounded && !isGrappling && !isDashing && !isDead)
        {
            coyoteTimeCounter = coyoteTime;
            wallSlideTime = wallSlideTimeMax;
            canDash = true;
            canGrapple = true;
            hasJumped = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump buffer
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // -- Normal Jump --
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !hasJumped && Time.timeScale == 1f)
        {
            DoJump(Vector2.up);
            hasJumped = true;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            audio.PlayOneShot(jumpSound);
        }

        // -- WALL SLIDE --
        isHoldingGrab = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton1);
        
        if (isTouchingWall && !grounded && wallSlideTime > 0 && isHoldingGrab)
        {
            isWallSliding = true;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))
            {
                // Dirección opuesta a la pared
                float jumpDirectionX = wallNormal.x;

                // Resetear velocidad
                rb.linearVelocity = Vector2.zero;

                // Fuerza del wall jump
                Vector2 wallJumpForce = new Vector2(jumpDirectionX * wallJumpForceX, wallJumpForceY);

                rb.AddForce(wallJumpForce, ForceMode2D.Impulse);

                // Flip visual
                if ((jumpDirectionX > 0 && facingRight) || (jumpDirectionX < 0 && !facingRight))
                {
                    Flip();
                }

                // Salir del wall slide
                isWallSliding = false;
                isTouchingWall = false;

                // Evita salto doble inmediato
                coyoteTimeCounter = 0;
                jumpBufferCounter = 0;

                isWallJumping = true;
                wallJumpTimer = wallJumpDuration;

                audio.PlayOneShot(jumpSound);

                return;
            }
            // -- Go Up/Down while wall sliding --
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

        // -- DASH --
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.JoystickButton2)) && canDash && !isGrappling && (horizontal != 0 || vertical != 0) && !isDead)
        {
            canDash = false;
            isDashing = true;
            dashTime = dashDuration;

            rb.linearVelocity = Vector2.zero;

            Vector2 dashDirection = new Vector2(horizontal, vertical);

            if (dashDirection == Vector2.zero)
            {
                dashDirection = facingRight ? Vector2.right : Vector2.left;
            }

            dashDirection.Normalize();

            dashVelocity = dashDirection * dashForce;
            rb.linearVelocity = dashVelocity;

            audio.PlayOneShot(dashSound);
        }
        if (isDashing)
        {
            rb.gravityScale = 0;
            rb.linearVelocity = dashVelocity;
            dashTime -= Time.deltaTime;

            if (dashTime <= 0)
            {
                isDashing = false;
                rb.linearVelocity = new Vector2(0, 0);
                rb.gravityScale = 1;
            }
        }


        // -- VARIABLE JUMP --
        /*
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.JoystickButton0))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
        */
        if (!isDashing && !isGrappling && !externalLaunch && !isDead)
        {
            // Caso 1: Cayendo (Caída rápida y pesada)
            if (rb.linearVelocity.y < 0)
            {
                rb.gravityScale = normalGravityScale * fallMultiplier;
            }
            // Caso 2: En el pico del salto (Efecto Suspensión / Hover)
            else if (rb.linearVelocity.y > 0 && Mathf.Abs(rb.linearVelocity.y) < peakHoverThreshold)
            {
                rb.gravityScale = peakHoverGravity;
            }
            // Caso 3: Soltó el botón de salto antes de tiempo (Corta el salto rápido)
            else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space) && !Input.GetKey(KeyCode.JoystickButton0))
            {
                rb.gravityScale = normalGravityScale * lowJumpMultiplier;
            }
            // Caso 4: Subida normal inicial o en el suelo
            else
            {
                rb.gravityScale = normalGravityScale;
            }
        }
        // FLIP PLAYER
        if (horizontal < 0 && !facingRight && Time.timeScale != 0 && !isDead)
        {
            Flip();
        }
        else if (horizontal > 0 && facingRight && Time.timeScale != 0 && !isDead)
        {
            Flip();
        }

        // ANIMACIONES y sonido de caminar
        if (horizontal != 0)
        {
            anim.SetBool("IsWalking", true);

            if (!walkAudio.isPlaying && grounded)
            {
                walkAudio.PlayOneShot(walkGrassSound);
            }
        }
        else
        {
            anim.SetBool("IsWalking", false);
        }
        if (rb.linearVelocity.y > 0)
        {
            anim.SetBool("IsJump", true);
            anim.SetBool("IsFall", false);
        }
        if (rb.linearVelocity.y < 0)
        {
            anim.SetBool("IsFall", true);
            anim.SetBool("IsJump", false);
        }
        if (rb.linearVelocity.y == 0)
        {
            anim.SetBool("IsFall", false);
            anim.SetBool("IsJump", false);
        }


        if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.JoystickButton4)) && canGrapple && !isDashing && !isGrappling && !isDead)
        {
            StartGrapple();
        }

        if (isGrappling)
        {
            grappleTimer -= Time.deltaTime;

            if (grappleTimer <= 0f)
            {
                StopGrapple();
                return;
            }

            grappleline.SetPosition(0, new Vector3(transform.position.x, transform.position.y, 0));
            grappleline.SetPosition(1, new Vector3(grapplePoint.x, grapplePoint.y, 0));

            if (Vector2.Distance(transform.position, grapplePoint) < 1f)
            {
                StopGrapple();
            }
        }

        if (wallSlideTime <= 0f)
        {
            spriteRenderer.color = Color.red;
        }
        else if (wallSlideTime <= 2f)
        {
            spriteRenderer.color = Color.cyan;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }

        if (grappleObject != null)
        {
            grappleObject.SetActive(canGrapple);
        }

        // -- DEBUG --
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // Siguiente escena
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            // Escena anterior
            int previousScene = SceneManager.GetActiveScene().buildIndex - 1;

            if (previousScene >= 0)
            {
                SceneManager.LoadScene(previousScene);
            }
        }

        if (isDead) return;
    }

    private void FixedUpdate()
    {
        //fall max velocity
        if (rb.linearVelocityY < -maxFallVelocity)
        {
            rb.linearVelocityY = -maxFallVelocity;
        }

        //Portal
        if (externalLaunch)
        {
            externalLaunchTime -= Time.fixedDeltaTime;

            if (externalLaunchTime <= 0)
            {
                externalLaunch = false;
            }

            return;
        }

        if (isWallJumping)
        {
            wallJumpTimer -= Time.fixedDeltaTime;

            if (wallJumpTimer <= 0)
            {
                isWallJumping = false;
            }

            return;
        }

        if (isDashing) return;

        if (isGrappling)
        {
            Vector2 direction = (grapplePoint - (Vector2)transform.position).normalized;

            rb.linearVelocity = direction * grappleSpeed;

            anim.SetBool("IsGrappling", true);

            DestroyBonesWhileGrappling();
        }
        else
        {
            if (isWallSliding)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else if (!isDead)
            {

                rb.linearVelocity = new Vector2(horizontal * speed * Time.fixedDeltaTime, rb.linearVelocity.y);
            }
            anim.SetBool("IsGrappling", false); 
        }

    }

    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void DoJump(Vector2 direction)
    {
        rb.AddForce(direction * jumpForce, ForceMode2D.Impulse);
    }

    private void StartGrapple()
    {
        grappleTimer = grappleDuration;

        Vector2 direction;

        if (controllerAim != Vector2.zero)
        {
            direction = controllerAim;

            float controllerAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float snappedControllerAngle = Mathf.Round(controllerAngle / 45f) * 45f;
            float controllerRad = snappedControllerAngle * Mathf.Deg2Rad;

            direction = new Vector2(Mathf.Cos(controllerRad), Mathf.Sin(controllerRad));
        }
        else
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 rawDirection = (mousePos - (Vector2)transform.position).normalized;

            float angle = Mathf.Atan2(rawDirection.y, rawDirection.x) * Mathf.Rad2Deg;
            float snappedAngle = Mathf.Round(angle / 45f) * 45f;
            float rad = snappedAngle * Mathf.Deg2Rad;

            direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, grappleMaxDistance, grappleLayer);

        if (hit.collider != null)
        {
            grapplePoint = hit.point;
            audio.PlayOneShot(grappleHookSound);
        }
        else
        {
            grapplePoint = (Vector2)transform.position + direction * grappleMaxDistance;
            audio.PlayOneShot(grappleLaunchSound);
        }

        isGrappling = true;
        grappleline.enabled = true;
        canGrapple = false;
    }

    public void StopGrapple()
    {
        isGrappling = false;
        grappleline.enabled = false;
        rb.linearVelocity = new Vector2(0, 0);
        rb.gravityScale = 1;
    }

    //Usada por Portal
    public void StopDash()
    {
        isDashing = false;
        dashTime = 0f;

        rb.gravityScale = 1;
        rb.linearVelocity = Vector2.zero;
    }

    //usada por DashGhost
    public bool CanDash()
    {
        return canDash;
    }
    private void DestroyBoneWall(GameObject bone)
    {
        if (bone == null || !bone.CompareTag("Bone"))
            return;

        Collider2D boneCollider = bone.GetComponent<Collider2D>();

        if (boneCollider == null)
            return;

       ContactFilter2D filter = new ContactFilter2D();
       filter.NoFilter();

        Collider2D[] results = new Collider2D[20];

        int count = boneCollider.Overlap(filter, results);

        Destroy(bone);

        for (int i = 0; i < count; i++)
        {
            if (results[i] != null &&
                results[i].CompareTag("Bone") &&
                results[i].gameObject != bone)
            {
                DestroyBoneWall(results[i].gameObject);
            }
        }
    }
    private void DestroyBonesWhileGrappling()
    {
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null) return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();

        Collider2D[] results = new Collider2D[20];
        int count = playerCollider.OverlapCollider(filter, results);

        for (int i = 0; i < count; i++)
        {
            if (results[i] != null && results[i].CompareTag("Bone"))
            {
                DestroyBoneWall(results[i].gameObject);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            isDead = true;
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(0, 0);
            anim.SetTrigger("IsDeath");
        }

        if ((isDashing || isGrappling) && collision.gameObject.CompareTag("Bone"))
        {
            Collider2D playerCollision = GetComponent<Collider2D>();
            Collider2D boneCollision = collision.collider;

            // Ignora la colisión inmediatamente
            Physics2D.IgnoreCollision(playerCollision, boneCollision, true);

            DestroyBoneWall(collision.gameObject);
            rb.linearVelocity = dashVelocity;

        }

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Bone"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
               
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    isTouchingWall = true;
                    wallNormal = contact.normal;
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
            audio.PlayOneShot(grappleRecoverSound);
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("Estancia"))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Bone"))
        {
            isTouchingWall = false;
        }

    }

    //Usada por la animacion de Death
    private void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}