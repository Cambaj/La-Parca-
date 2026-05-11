using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]

    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference dashAction;
    [SerializeField] private InputActionReference grableLeftAction; //Tecla Q
    [SerializeField] private InputActionReference grableRightAction; //Tecla E
    [SerializeField] private GrapplingHook grappleScript; //Aqui esta el gancho

    [SerializeField] private Rigidbody2D playerRigidbody;
    [SerializeField] private AnimationController animController;  //Tener en cuenta este detalle 
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Ground Checking")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Salto variable")]
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float fallMultiplier = 2.5f;
    
    //Ver si quito o mantengo el coyote time 
    [Header("Jump Buffer & Coyote Time")]
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float coyoteTime = 0.15f;

    private float jumpBufferCounter;
    private float coyoteTimeCounter;

    private float horizontal;
    private float vertical;

    //Ver si esto lo mando accion del jugador o lo dejo aca 
    [Header("Wall Setting")]
    [SerializeField] private float wallSlideSpeed = 10f;
    [SerializeField] private float wallClimbSpeed = 4f;
    [SerializeField] private Transform wallCheckTransform;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private LayerMask wallLayer;


    [Header("Inputs")]
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool wallAtLeft;
    private bool wallAtRight;
    private bool isDashing;
    private bool canDash = true;
    private bool hasAirDashed = false;
    private float originalGravity;

    [Header("Player States")]
    public bool Is_idle;
    public bool Is_moving;
    public bool Is_jumping;
    public bool Is_dashing;
    public bool Is_falling;
    public bool Is_wallSliding; //Ver si se hace animacion de esto

    private static PlayerMovement instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else 
        {
            Destroy(gameObject);
            return;
        }
            originalGravity = playerRigidbody.gravityScale;
    }



    private void OnEnable()
    {
        SceneManager.sceneLoaded += AlCargarEscena;

        moveAction.action.Enable();
        jumpAction.action.Enable();
        dashAction.action.Enable();
        grableLeftAction.action.Enable();
        grableRightAction.action.Enable();

        jumpAction.action.started += HandleJumpInput;
        dashAction.action.started += HandleDashInput;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= AlCargarEscena;

        moveAction.action.Disable();
        jumpAction.action.Disable();
        dashAction.action.Disable();

        jumpAction.action.started -= HandleJumpInput;
        dashAction.action.started -= HandleDashInput;
    }

    private void AlCargarEscena(Scene scene, LoadSceneMode mode)
    {
        GameObject punto = GameObject.Find("SpawnPoint");
        if (punto != null)
        {
            transform.position = punto.transform.position;
            playerRigidbody.linearVelocity = Vector2.zero;
        }
    }   

    private void Update()
    {
        if (grappleScript != null && grappleScript.IsGrappling) return;
        if (isDashing) return;

        moveInput = moveAction.action.ReadValue<Vector2>();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

       float direction = spriteRenderer.flipX ? -1 : 1;
       isTouchingWall = Physics2D.Raycast(wallCheckTransform.position, Vector2.right * direction, 0.1f, wallLayer);

       wallAtLeft = Physics2D.Raycast(wallCheckTransform.position, Vector2.left, wallCheckDistance, wallLayer);
       wallAtRight = Physics2D.Raycast(wallCheckTransform.position, Vector2.right, wallCheckDistance, wallLayer);

        if(wallAtLeft) Debug.Log("Pared detectada a la izquierda");
        if(wallAtRight) Debug.Log("Pared detectada a la derecha");

        if (isGrounded) hasAirDashed = false;

        //Aqui se da vuelta el sprite
        if(!Is_wallSliding)
        {
            if (moveInput.x > 0.0f) spriteRenderer.flipX = false;
            else if (moveInput.x < 0) spriteRenderer.flipX = true;
        }

        HandleWallSlide();
        UpdateStates();
        ApplyVariableJump();

    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        if (Is_wallSliding)
        {
            playerRigidbody.linearVelocity = new Vector2(0, moveInput.y * wallClimbSpeed);
        }
        else
        {
            playerRigidbody.linearVelocity = new Vector2(moveInput.x * moveSpeed, playerRigidbody.linearVelocity.y);
        
        }
              
    }

    private void HandleWallSlide()
    {
        bool grabbingLeft = wallAtLeft && grableLeftAction.action.IsPressed();
        bool grabbingRight = wallAtRight && grableRightAction.action.IsPressed();

        if ((grabbingLeft || grabbingRight) && !isGrounded)
        {
            Is_wallSliding = true;
            playerRigidbody.gravityScale = 0;

            if (grabbingLeft) spriteRenderer.flipX = true;
            if (grabbingRight) spriteRenderer.flipX = false;
        }
        else
        {
            Is_wallSliding = false;
            playerRigidbody.gravityScale = originalGravity; 
        }
    }

    private void UpdateStates()
    { 
        Is_moving = Mathf.Abs(moveInput.x) > 0.1f;
        Is_jumping = !isGrounded && playerRigidbody.linearVelocity.y > 0.1f;
        Is_falling = !isGrounded && playerRigidbody.linearVelocity.y < -0.1f;
        Is_dashing = isDashing;
        Is_idle = !Is_moving && !Is_jumping && !Is_falling && !Is_dashing;
    }

    //JUMP 
    private void HandleJumpInput(InputAction.CallbackContext context)
    {
        if (isDashing) return;
      
        if (isGrounded && !isDashing)
        {
            playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, jumpForce);
        }
        else if (Is_wallSliding)
        {
            float jumpDirectionX = spriteRenderer.flipX ? 1 : -1;
            playerRigidbody.linearVelocity = new Vector2(jumpDirectionX * moveSpeed, jumpForce);

            spriteRenderer.flipX = !spriteRenderer.flipX;
            Is_wallSliding = false;
        }
    }

    // ---- VARIABLE JUMP ----
 private void ApplyVariableJump()
    {
        if (playerRigidbody.linearVelocity.y < 0)
        {
            playerRigidbody.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (playerRigidbody.linearVelocity.y > 0 && !jumpAction.action.IsPressed())
        {
           playerRigidbody.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }


//DASH

private void HandleDashInput(InputAction.CallbackContext context)
    {
        if (!isDashing && canDash && (isGrounded || !hasAirDashed))
        {
            if (!isGrounded) hasAirDashed = true;
            StartCoroutine(DashCoroutine());
        }
    }

    
    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        canDash = false;
        float oldGravity = playerRigidbody.gravityScale;
        playerRigidbody.gravityScale = 0f;

        Vector2 dashDir = moveInput.magnitude > 0.1f ? moveInput.normalized : new Vector2(spriteRenderer.flipX ? -1 : 1, 0);
        playerRigidbody.linearVelocity = dashDir * dashForce;

        yield return new WaitForSeconds(dashDuration);

        playerRigidbody.gravityScale = originalGravity;
        playerRigidbody.linearVelocity = Vector2.zero;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (wallCheckTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(wallCheckTransform.position, Vector2.left * wallCheckDistance);
            Gizmos.DrawRay(wallCheckTransform.position, Vector2.right * wallCheckDistance);
        }
    }

    //Script de muerte y logica del la pared de hueso

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            Muerte();
        }

        bool estaAtacando = Is_dashing || (grappleScript != null && grappleScript.IsGrappling);

        if (estaAtacando && collision.gameObject.CompareTag("Bone"))
        {
            Destroy(collision.gameObject);
        }
    }
    
    private void Muerte()
    {
        playerRigidbody.linearVelocity = Vector2.zero;
        playerRigidbody.simulated = false;

        if(animController != null)
        {
            animController.PlayDeath();
        }
    }

    public void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

  //Entidad de boost

    /*
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
    */

}