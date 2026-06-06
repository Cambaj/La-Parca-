using UnityEngine;

public class GranadeSpawner : MonoBehaviour
{
    [Header("Configuracion del Spawn")]
    [SerializeField] private GameObject granadePrefab;
    [SerializeField] private float spawnCooldown = 3f; // Tiempo para que reapareza la granada una vez usada

    private float cooldownTimer = 0f;
    private bool isAreaOccupied = false;

    private void Update()
    {
        if (isAreaOccupied)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= spawnCooldown)
            {
                SpawnGranade();
            }
        }
    }

    private void SpawnGranade()
    {
        Instantiate(granadePrefab, transform.position, Quaternion.identity);
        isAreaOccupied = true;
        cooldownTimer = 0f;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Granade"))
        {
            isAreaOccupied = true;
        }
    }

    private void OnTriggerExit2D(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Granade"))
        {
            isAreaOccupied = false;
            cooldownTimer = 0f; // Reinicia el timer para que la granada reaparezca inmediatamente
        }
    }

}
