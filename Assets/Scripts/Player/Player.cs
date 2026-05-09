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

    [Header("Dash")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    private bool canDash = true;
    private bool isDashing = false;
    private float dashTime;
    private bool canGrapple = true;

    [Header("Audio")]
    [SerializeField] private AudioClip JumpSound;

    [Header("Mando")]
    [SerializeField] private bool useController = true;

    private Vector2 controllerAim;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    Animator anim;
    AudioSource audio;

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
        if (grounded)
        {
            coyoteTimeCounter = coyoteTime;
            wallSlideTime = wallSlideTimeMax;
            canDash = true;
            canGrapple = true;
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

        // ---- Normal Jump ----
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && Time.timeScale == 1f)
        {
            DoJump(Vector2.up);
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
            audio.PlayOneShot(JumpSound);
        }
        else
        {
        }

            // ---- WALL SLIDE ----
            bool isHoldingGrab = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton1);
        
        if (wallNormal.x > 0 && horizontal < 0) isHoldingGrab = true;
        if (wallNormal.x < -0 && horizontal > 0) isHoldingGrab = true;
        

        if (isTouchingWall && !grounded && wallSlideTime > 0 && isHoldingGrab)
        {
            isWallSliding = true;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                float jumpDirectionX= wallNormal.x;
                rb.linearVelocity = new Vector2(jumpDirectionX * speed, jumpForce);

                if ((jumpDirectionX > 0 && !facingRight) || (jumpDirectionX < 0 && facingRight))
                {
                    Flip();
                }
                isWallSliding = false;
                wallSlideTime = 0;
                return;
            }
            // ---- Go Up or down while wall sliding ----
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
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.JoystickButton2)) && canDash && !grounded && !isGrappling)
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


        // ---- VARIABLE JUMP ----
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        // FLIP PLAYER
        if (horizontal < 0 && !facingRight && Time.timeScale != 0)
        {
            Flip();
        }
        else if (horizontal > 0 && facingRight && Time.timeScale != 0)
        {
            Flip();
        }

        // ANIMACIONES
        if (horizontal != 0)
        {
            anim.SetBool("IsWalking", true);
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


        if ((Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.JoystickButton4)) && canGrapple && !isDashing)
        {
            StartGrapple();
        }


        if (isGrappling)
        {
           
            grappleline.SetPosition(0, new Vector3(transform.position.x, transform.position.y, 0));
            grappleline.SetPosition(1, new Vector3(grapplePoint.x, grapplePoint.y, 0));

            if (Vector2.Distance(transform.position, grapplePoint) < 1f)
            {
                StopGrapple();
            }
        }

        if (!canDash && isWallSliding && wallSlideTime <= 2f)
        {
            spriteRenderer.color = Color.red;
        }
        else if (isWallSliding && wallSlideTime <= 2f)
        {
            spriteRenderer.color = Color.cyan;
        }
        else if (!canDash)
        {
            spriteRenderer.color = Color.yellow;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }

        ToggleChildren(canGrapple);
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        if (isGrappling)
        {
            Vector2 direction = (grapplePoint - (Vector2)transform.position).normalized;

            rb.linearVelocity = direction * grappleSpeed;

            anim.SetBool("IsGrappling", true);
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
        Vector2 direction;

        if (useController && controllerAim != Vector2.zero)
        {
            direction = controllerAim;
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

        float controllerAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float snappedControllerAngle = Mathf.Round(controllerAngle / 45f) * 45f;
        float controllerRad = snappedControllerAngle * Mathf.Deg2Rad;

        direction = new Vector2(Mathf.Cos(controllerRad), Mathf.Sin(controllerRad));

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, grappleMaxDistance, grappleLayer);

        if (hit.collider != null)
        {
            grapplePoint = hit.point;
        }
        else
        {
            grapplePoint = (Vector2)transform.position + direction * grappleMaxDistance;
        }

        isGrappling = true;
        grappleline.enabled = true;
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            anim.SetTrigger("IsDeath");
        }

        if ((isDashing || isGrappling) && collision.gameObject.CompareTag("Bone"))
        {
            DestroyBoneWall(collision.gameObject);
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