using UnityEngine;

public class MetaNivel : MonoBehaviour
{

    [Header("Configuración de Progreso (Guardado)")]
    [Tooltip("El número de reino actual de este nivel (ej: Hambruna = 1, Guerra = 2, Peste = 3, Muerte = 4).")]
    [SerializeField] private int numeroReino = 1;
    [Tooltip("El número de nivel actual dentro de este reino (ej: si es Nivel 3, pones 3).")]
    [SerializeField] private int numeroNivelActual = 1;
    [Tooltip("La cantidad total de niveles que tiene este reino específico en la cuadrícula.")]
    [SerializeField] private int totalNivelesEnEsteReino = 6;

    // Este método lo va a llamar el Player justo antes de cambiar de escena
    public void GuardarProgreso()
    {
        int siguienteReino = numeroReino;
        int siguienteNivel = numeroNivelActual + 1;

        // Si el jugador superó el último nivel de la fila de este reino, salta al Nivel 1 del Siguiente Reino
        if (numeroNivelActual >= totalNivelesEnEsteReino)
        {
            siguienteReino = numeroReino + 1;
            siguienteNivel = 1;
        }

        // Creamos la clave exacta con la nomenclatura dinámica de tu SelectorManager: "Nivel_1-2", "Nivel_1-3", etc.
        string claveSiguiente = "Nivel_" + siguienteReino + "-" + siguienteNivel;

        // Guardamos un 1 en PlayerPrefs para marcarlo como desbloqueado en la PC
        PlayerPrefs.SetInt(claveSiguiente, 1);
        PlayerPrefs.Save(); // Asegura la escritura inmediata en el disco

        Debug.Log($" [PROGRESO] ¡Nivel superado! Se ha guardado la clave: {claveSiguiente} = Desbloqueado.");
    }
}
