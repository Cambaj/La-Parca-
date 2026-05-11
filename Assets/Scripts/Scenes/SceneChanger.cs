using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneChanger : MonoBehaviour
{
    
    [Header("Configuracion de Escena")]
    [SerializeField] private string sceneName;

    private void OnTriggerEnter2D(Collider2D collision)
    {
         if (collision.CompareTag("Player"))
        {
            CargarSiguienteNivel();
        }
    }

    public void CargarSiguienteNivel()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            int indiceSiguiente = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(indiceSiguiente);
        }
    }
}
