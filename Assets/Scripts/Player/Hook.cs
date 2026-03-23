using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    [Header("Hook Configuration")]
    public float maxDistance = 10f;
    public LayerMask grappleLayer; // Capa de las paredes/techos

    private Vector2 grapplePoint;
    private DistanceJoint2D joint;
    private LineRenderer line;
    private Camera cam;

    void Start()
    {
        joint = GetComponent<DistanceJoint2D>();
        line = GetComponent<LineRenderer>();
        cam = Camera.main;

        joint.enabled = false;
        line.positionCount = 0;
    }

    void Update()
    {
        // Al presionar el click derecho (o el botón que elijas)
        if (Input.GetMouseButtonDown(1))
        {
            StartGrapple();
        }

        // Mientras mantienes presionado, dibujamos la cuerda
        if (Input.GetMouseButton(1) && joint.enabled)
        {
            line.SetPosition(0, transform.position);
            line.SetPosition(1, grapplePoint);
        }

        // Al soltar el botón
        if (Input.GetMouseButtonUp(1))
        {
            StopGrapple();
        }
    }

    void StartGrapple()
    {
        // Calculamos la dirección hacia el mouse
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;

        // Lanzamos un Raycast para ver si golpeamos algo enganchable
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, maxDistance, grappleLayer);

        if (hit.collider != null)
        {
            grapplePoint = hit.point;

            // Configuramos el Joint
            joint.connectedAnchor = grapplePoint;
            joint.distance = Vector2.Distance(transform.position, grapplePoint);
            joint.enabled = true;

            // Configuramos la línea
            line.positionCount = 2;
        }
    }

    void StopGrapple()
    {
        joint.enabled = false;
        line.positionCount = 0;
    }
}

