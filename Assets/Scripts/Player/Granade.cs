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

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip pickUpSound;
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private AudioClip explosionSound;

    private Transform equippedPlayer;
    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        audioSource = GetComponent<AudioSource>();
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

        audioSource.PlayOneShot(pickUpSound);
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

        audioSource.PlayOneShot(throwSound);
    }

    public bool WasThrown()
        {
        return wasThrown;
    }

    private IEnumerator IgnorePlayerTemporarily(Collider2D playerCol)
    {
        Physics2D.IgnoreCollision(col, playerCol, true);
        yield return null;
        /*
        yield return new WaitForSeconds(0.1f); // Tiempo corto para que se aleje del jugador
        if (col != null && playerCol != null)
        {
            Physics2D.IgnoreCollision(col, playerCol, false);
        }
        */
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
        Debug.Log($"[Granada] Explotando en {transform.position}. Radio: {explosionRadius}");

        isActive = false;

        if (!wasThrown && equippedPlayer != null)
        {
            var playerMovement = equippedPlayer.GetComponent<MonoBehaviour>();
            if (playerMovement != null)
            {
                playerMovement.SendMessage("ForceToDropGranade", SendMessageOptions.DontRequireReceiver);
                playerMovement.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
                AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            }
           
        }
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // --- NUEVO ESCANEO BRUTO (IGNORA CONFIGURACIÓN DE LAYERS DEL PROYECTO) ---
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter(); // Le dice a Unity: "No me importa la capa, busca TODO"

        Collider2D[] radiatedObjects = new Collider2D[30]; // Array temporal para guardar los impactos
        int count = Physics2D.OverlapCircle(transform.position, explosionRadius, filter, radiatedObjects);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = radiatedObjects[i];
            if (hit == null) continue;

            // Bloques destructibles
            DestroyableBlock nuevoBloque = hit.GetComponent<DestroyableBlock>();
            if (nuevoBloque != null)
            {
                nuevoBloque.Shatter();
                continue;
            }

            if (hit.CompareTag("Destroyable Wall"))
            {
                Destroy(hit.gameObject);
                continue;
            }

            // CONTROL DE DAÑO AL JUGADOR
            if (hit.CompareTag("Player"))
            {
                Debug.Log("<Color=Green><b>¡GRANADA DETECTÓ AL JUGADOR!</b></Color> Enviando señal de muerte...");
                hit.gameObject.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
            }
        }
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        Destroy(gameObject);

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

