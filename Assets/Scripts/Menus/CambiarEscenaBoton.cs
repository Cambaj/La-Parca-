using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CambiarEscenaBoton : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Escribe el nombre exacto de la escena a la que quieres viajar.")]
    [SerializeField] private string nombreEscenaDestino;

    /// <dt>MÉTODO PÚBLICO PARA EL ONCLICK</dt>
    public void CargarEscena()
    {
        if (!string.IsNullOrEmpty(nombreEscenaDestino))
        {
            // ¡EL DETALLE CRUCIAL! 
            // Restauramos el tiempo del motor a la normalidad por si venimos desde la pausa.
            Time.timeScale = 1f;

            Debug.Log(" Cargando escena: " + nombreEscenaDestino);
            SceneManager.LoadScene(nombreEscenaDestino);
        }
        else
        {
            Debug.LogWarning(" No se asignó ningún nombre de escena en el Inspector de " + gameObject.name);
        }
    }
}
