using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class GrapplingHook : MonoBehaviour
{

    [Header("Grappling Hook")]
    [SerializeField] private InputActionReference grappleAction;
    [SerializeField] private InputActionReference aimAction;

    [Header("Settings")]
    [SerializeField] private float grappleMaxDistance = 10f;
    [SerializeField] private float grappleSpeed = 20f;
    [SerializeField] private LayerMask grappleLayer;

    [Header("References")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Rigidbody2D rb;


    private Vector2 grapplePoint;
    private bool isGrappling;
    private float originalGravity;

    private void OnEnable()
    {
        grappleAction.action.Enable();
        aimAction.action.Enable();
        grappleAction.action.started += _ => StartGrapple();
        grappleAction.action.canceled += _ => StopGrapple();
    }

    private void OnDisable()
    {
        grappleAction.action.Disable();
        grappleAction.action.started -= _ => StartGrapple();    
        grappleAction.action.canceled -= _ => StopGrapple();
    }

    private void StartGrapple()
    {
        Vector2 direction = GetAimDirection();
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, grappleMaxDistance, grappleLayer);  

        if (hit.collider != null)
        {
            grapplePoint = hit.point;
            isGrappling = true;
            lineRenderer.enabled = true;

            originalGravity = rb.gravityScale;
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
        }
     
    }

    private Vector2 GetAimDirection()
    {
        Vector2 inputDir = aimAction.action.ReadValue<Vector2>();
        Vector2 finalDir;

        if (inputDir.sqrMagnitude > 0.1f)
        {
            finalDir = inputDir.normalized;
        }
        else 
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            finalDir = (mousePos - (Vector2)transform.position.normalized);
        }

        float angle = Mathf.Atan2(finalDir.y, finalDir.x) * Mathf.Rad2Deg;
        float snappedAngle = Mathf.Round(angle / 45f) * 45f;
        float rad = snappedAngle * Mathf.Deg2Rad;

        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private void StopGrapple()
    {
        if (!isGrappling) return;

        isGrappling = false;
        lineRenderer.enabled = false;
        rb.gravityScale = originalGravity;
    }

    private void Update()
    {
        if (!isGrappling) return;

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, grapplePoint);

        transform.position = Vector2.MoveTowards(transform.position, grapplePoint, grappleSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, grapplePoint) < 0.1f)
        {
            StopGrapple();
        }
    }
        public bool IsGrappling => isGrappling;
    
}

