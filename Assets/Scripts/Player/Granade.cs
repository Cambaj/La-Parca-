using UnityEngine;
using System.Collections;
public class Granade : MonoBehaviour
{
    [Header("Configuracion de Tiempo")]
    [SerializeField] private float explosionDelay = 5f;
    private float timer;
    private bool isActive = false;
    private bool wasThrown = false;

    [Header("Explosion y Destruccion")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private LayerMask destroyableLayer;
    [SerializeField] private GameObject explosionEffectPrefab;

    private Transform equippedPlayer;
    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Update()
    {
        if (isActive)
        {
            timer -= Time.deltaTime;
            if (!wasThrown && equippedPlayer != null)
            {
                transform.position = equippedPlayer.position + new Vector3(0, 0.8f, 0);
            }
            if (timer <= 0f)
            {
                Explotes();
            }
        }
    }

    public void PickUp(Transform player)
    {
        isActive = true;
        timer = explosionDelay;
        equippedPlayer = player;
        wasThrown = false;

        col.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
    }

    public void Throw(Vector2 impulseForce)
    {
        if (wasThrown) return;

        wasThrown = true;

        Collider2D playerCollider = equippedPlayer != null ? equippedPlayer.GetComponent<Collider2D>() : null;
        equippedPlayer = null;

        rb.bodyType = RigidbodyType2D.Dynamic;
        col.enabled = true;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(impulseForce, ForceMode2D.Impulse);

        if (playerCollider != null)
        {
            StartCoroutine(IgnorePlayerTemporarily(playerCollider));
        }
    }

    private IEnumerator IgnorePlayerTemporarily(Collider2D playerCol)
    {
        Physics2D.IgnoreCollision(col, playerCol, true);
        yield return new WaitForSeconds(0.15f);
        if (col != null && playerCol != null)
        {
            Physics2D.IgnoreCollision(col, playerCol, false);
        }
    }

    private void Explotes()
    {
        isActive = false;

        if (!wasThrown && equippedPlayer != null)
        {
            PlayerMovement player = equippedPlayer.GetComponent<PlayerMovement>();
            if (player != null) player.ForceToDropGranade();
        }

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Realiza la búsqueda física en el radio asignado
        Collider2D[] impactedObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius, destroyableLayer);

        foreach (Collider2D hit in impactedObjects)
        {
            // Busca si el objeto impactado tiene el script de la nueva pared
            DestroyableBlock nuevoBloque = hit.GetComponent<DestroyableBlock>();
            if (nuevoBloque != null)
            {
                nuevoBloque.Shatter();
                continue;
            }

            // Compatibilidad por Tag con tus estructuras viejas si las dejás en la misma capa
            if (hit.CompareTag("Destroyable Wall") || hit.CompareTag("Bone"))
            {
                Destroy(hit.gameObject);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

