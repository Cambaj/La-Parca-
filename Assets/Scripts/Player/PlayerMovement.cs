using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    {

    [Header("References")]

    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference dashAction;

    [SerializeField] private Rigidbody2D playerRigidbody;
    //[SerializeField] private Animator_Controller animController;
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

    [Header("Inputs")]
    private Vector2 moveInput;
    private bool isGrounded;
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

    private void Awake()
    {
        originalGravity = playerRigidbody.gravityScale;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        dashAction.action.Enable();

        jumpAction.action.started += HandleJumpInput;
        dashAction.action.started += HandleDashInput;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
        dashAction.action.Disable();

        jumpAction.action.started -= HandleJumpInput;
        dashAction.action.started -= HandleDashInput;
    }

    private void Update()
    {
        if (isDashing) return;

        moveInput = moveAction.action.ReadValue<Vector2>();
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            hasAirDashed = false;
        }

        if (moveInput.x > 0) spriteRenderer.flipX = false;
        else if (moveInput.x < 0) spriteRenderer.flipX = true;

        Is_moving = Mathf.Abs(moveInput.x) > 0.1f;
        Is_jumping = !isGrounded && playerRigidbody.linearVelocity.y > 0.1f;
        Is_falling = !isGrounded && playerRigidbody.linearVelocity.y < -0.1f;
        Is_dashing = isDashing;
        Is_idle = !Is_moving && !Is_jumping && !Is_falling && !Is_dashing;

        bool falling = !isGrounded && playerRigidbody.linearVelocity.y < -0.1f;

        if (AnimController != null)
        {
            animController.UpdateAnimation(Is_moving, Is_idle, Is_jumping, Is_falling, Is_dashing);
        }

    }
    //JUMP 
    private void FixedUpdate()
    {
        if (isDashing) return;

        playerRigidbody.linearVelocity = new Vector2(moveInput.x * moveSpeed, playerRigidbody.linearVelocity.y);
    }

    private void HandleJumpInput(InputAction.CallbackContext context)
    {
        if (isGrounded && !isDashing)
        {
            playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, jumpForce);
        }
    }



    //DASH
    private void HandleDashInput(InputAction.CallbackContext context)
    {
        if (!isDashing && canDash && (isGrounded || !hasAirDashed))
        {
            StartCoroutine(DashCoroutine());
        }
    }
    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        canDash = false;

        playerRigidbody.gravityScale = 0f;

        float dashDirection = spriteRenderer.flipX ? -1 : 1f;
        if (moveInput.x != 0) dashDirection = Mathf.Sign(moveInput.x);

        playerRigidbody.linearVelocity = new Vector2(dashDirection * dashForce, 0f);

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
    }

}
}
