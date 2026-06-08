using UnityEngine;

public class SoulBone : MonoBehaviour
{
    [SerializeField] private float speed = 15f;

    private HungrySoul target;

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

        Vector2 dir =
            ((Vector2)target.transform.position -
             (Vector2)transform.position).normalized;

        transform.position +=
            (Vector3)(dir * speed * Time.deltaTime);
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