using UnityEngine;

public class SoulBone : MonoBehaviour
{
    [SerializeField] private float speed = 15f;

    private HungrySoul target;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Initialize(HungrySoul soul)
    {
        target = soul;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 dir = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;
        rb.linearVelocity = dir * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ground") || ((1 << collision.gameObject.layer) & LayerMask.GetMask("Ground")) != 0)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HungrySoul soul = other.GetComponent<HungrySoul>();

        if (soul != null)
        {
            soul.PinSoul();
            Destroy(gameObject);
        }
    }
}