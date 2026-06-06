using UnityEngine;
using System.Collections;
public class Granade : MonoBehaviour
{
    [Header("Configuracion de Tiempo")]
    [SerializeField] private float explosionDelay;
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
        yield return new WaitForSeconds(0.1f); // Tiempo corto para que se aleje del jugador
        if (col != null && playerCol != null)
        {
            Physics2D.IgnoreCollision(col, playerCol, false);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (wasThrown)
        {
            Explotes();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (wasThrown && !other.CompareTag("Player") && !other.CompareTag("Entity"))
        {
            Explotes();
        }
    }
    private void Explotes()
    {
        isActive = false;

        if (!wasThrown && equippedPlayer != null)
        {
            PlayerMovement player = equippedPlayer.GetComponent<PlayerMovement>();
            if (player != null)
            {
                 player.ForceToDropGranade();
                 player.Die(); //El jugador muere si la granda explota en la mano
            }
           
        }

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Detectar objetos en el radio de explosión (Paredes agrietadas y el Jugador)
        Collider2D[] radiatedObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in radiatedObjects)
        {
            DestroyableBlock nuevoBloque = hit.GetComponent<DestroyableBlock>();
            if (nuevoBloque != null)
            {
                    nuevoBloque.Shatter();
                    continue;   
            }

            // Compatibilidad por Tag con tus estructuras viejas si las dejás en la misma capa
            if (hit.CompareTag("Destroyable Wall") )
            {
                Destroy(hit.gameObject);
                continue;
            }
            if (hit.CompareTag("Player"))
            {
                PlayerMovement player = hit.GetComponent<PlayerMovement>();
                if (player != null)
                {
                    player.Die();
                }
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

