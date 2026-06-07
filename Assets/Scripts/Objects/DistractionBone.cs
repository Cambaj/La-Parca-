using UnityEngine;

public class DistractionBone : MonoBehaviour
{
    [Header("Configuracion de Distraccion")]
    [SerializeField] private float distractionDuration = 5f; // Duracion de la distraccion  

    private void Start()
    {
        HungrySoul[] souls = Object.FindObjectsByType<HungrySoul>(FindObjectsSortMode.None);

        foreach (HungrySoul soul in souls)
        {
            soul.Distract(transform);
        }

        Destroy(gameObject, distractionDuration);

    }
}
