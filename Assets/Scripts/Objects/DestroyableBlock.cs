using UnityEngine;

public class DestroyableBlock : MonoBehaviour
{
    [Header("Efectos Visuales (Opcional)")]
    [SerializeField] private GameObject debriseEffectPrefab;

    public void Shatter()
    {
       if (debriseEffectPrefab != null)
        {
            Instantiate(debriseEffectPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}


