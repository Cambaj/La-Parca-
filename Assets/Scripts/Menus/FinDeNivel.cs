using UnityEngine;
using UnityEngine.SceneManagement;

public class FinDeNivel : MonoBehaviour
{
    [Header("Configuración del Nivel Actual")]
    [Tooltip("El reino en el que está jugando el usuario actualmente.")]
    [SerializeField] private int reinoActual = 1;
    [Tooltip("El número de nivel dentro de este reino.")]
    [SerializeField] private int nivelActual = 1;
    [Tooltip("La cantidad total de niveles que tiene este reino específico (ej: 6).")]
    [SerializeField] private int totalNivelesEnEsteReino = 6;

    [Header("Escena a Cargar")]
    [Tooltip("Nombre de la escena a la que viaja el jugador al ganar (ej: SelectorNiveles o el siguiente nivel directo).")]
    [SerializeField] private string escenaSiguiente = "SelectorNiveles";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verificar si lo que entró al trigger es el jugador
        if (collision.CompareTag("Player"))
        {
            DesbloquearSiguienteNivel();
            CargarSiguienteEscena();
        }
    }

    private void DesbloquearSiguienteNivel()
    {
        int siguienteReino = reinoActual;
        int siguienteNivel = nivelActual + 1;

        // Si el jugador superó el último nivel de este reino, salta al Nivel 1 del Siguiente Reino
        if (nivelActual >= totalNivelesEnEsteReino)
        {
            siguienteReino = reinoActual + 1;
            siguienteNivel = 1;
            Debug.Log(" ¡Reino {reinoActual} Completado! Avanzando al Reino {siguienteReino}");
        }

        // Creamos la clave dinámica idéntica a la que busca tu SelectorManager
        // Ejemplo: "Nivel_1-2" o "Nivel_2-1"
        string claveSiguiente = "Nivel_" + siguienteReino + "-" + siguienteNivel;

        // Guardamos un 1 en PlayerPrefs para marcarlo como desbloqueado
        PlayerPrefs.SetInt(claveSiguiente, 1);
        PlayerPrefs.Save(); // Asegura que los datos se escriban en el disco de la PC inmediatamente

        Debug.Log("Progreso Guardado: {claveSiguiente} ahora está desbloqueado.");
    }

    private void CargarSiguienteEscena()
    {
        if (!string.IsNullOrEmpty(escenaSiguiente))
        {
            SceneManager.LoadScene(escenaSiguiente);
        }
    }
}
