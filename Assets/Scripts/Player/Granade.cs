using UnityEngine;

public class Granade : MonoBehaviour
{
    [Header("Configuracion de Tiempo")]
    [SerializeField] private float explosionDelay = 5f;
    private float timer;
    private bool isActive = false;
    private bool wasThrown = false;

    [Header("Explosion y Destruccion")]
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private LayerMask DestroyableWall;
    [SerializeField] private GameObject explosionEffectPrefab;

    private Transform equippedPlayer;
    private Rigidbody2D rb;
    private Collider2D col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; //Empieza la granada quieta en mapa y espera a que sea reccogida
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

        col.enabled = false; //Se desactiva colisiones mientras el jugador tiene la granada
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero; //Detiene cualquier movimiento previo
    }

    public void Throw(Vector2 impulseForce)
    {
        wasThrown = true;
        equippedPlayer = null;

        col.enabled = true; //Se reactiva colisiones para que rebote en las paredes
        rb.bodyType = RigidbodyType2D.Dynamic; //Permite que la granada se mueva libremente
        rb.linearVelocity = impulseForce;
        rb.AddForce(impulseForce, ForceMode2D.Impulse);
        //Ver aca que se le puede cambiar
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
        
        ContactFilter2D filter= new ContactFilter2D();
        filter.layerMask = DestroyableWall;
        filter.useLayerMask = true;
        

        Collider2D[] impactedObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius, DestroyableWall);

        foreach (Collider2D hit in impactedObjects)
        {
            if (hit.CompareTag("DestroyableWall")  || hit.CompareTag("bones"))//Cambiarle de Tag
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
