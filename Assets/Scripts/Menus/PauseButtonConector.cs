using UnityEngine;
using UnityEngine.UI;

public class PauseButtonConector : MonoBehaviour
{
    [Header("Botones del Prefab de Pausa")]
    [SerializeField] private Button botonReanudar;
    [SerializeField] private Button botonReiniciarNivel;
    [SerializeField] private Button botonReiniciarReino;
    [SerializeField] private Button botonSelectorNiveles; 
    [SerializeField] private Button botonOpciones;
    [SerializeField] private Button botonSalirJuego;
    [SerializeField] private Button botonMenuPrincipal;

    [Header("Configuración del Reino")]
    [Tooltip("Escribe el nombre exacto de la primera estancia de este reino (ej: estancia 1)")]
    [SerializeField] private string primerNivelDelReino;

    void Start()
    {
        // Al arrancar el nivel, este script busca al CanvasManager de la escena actual
        CanvasManager canvasManager = CanvasManager.Instance;

        if (canvasManager != null)
        {
            // 1. Reanudar juego
            if (botonReanudar != null)
                botonReanudar.onClick.AddListener(() => canvasManager.TogglePause());

            // 2. Reiniciar el nivel actual en el que está el jugador
            if (botonReiniciarNivel != null)
                botonReiniciarNivel.onClick.AddListener(() => canvasManager.ReiniciarNivelActual());

            // 3. Reiniciar el Reino completo desde su primera estancia
            if (botonReiniciarReino != null && !string.IsNullOrEmpty(primerNivelDelReino))
                botonReiniciarReino.onClick.AddListener(() => canvasManager.ReiniciarReino(primerNivelDelReino));

            // 4. Volver directo al Selector de Niveles (Menú Principal con el panel abierto)
            if (botonSelectorNiveles != null)
                botonSelectorNiveles.onClick.AddListener(() => {
                    // Despausamos el tiempo para que el menú no se quede congelado
                    Time.timeScale = 1f;
                    UnityEngine.AudioListener.pause = false;
                    canvasManager.CargarEscena("StartMenu");
                    // Nota: Para que se abra el panel del selector directo al volver, 
                    // puedes activar el flag en el Start de tu CanvasManager principal.
                });

            // 5. Abrir Panel de Opciones (Volumen/Cheats)
            if (botonOpciones != null && canvasManager.OptionsPanel != null)
                botonOpciones.onClick.AddListener(() => canvasManager.CambiarPanel(canvasManager.OptionsPanel));

            // 6. Salir del juego por completo
            if (botonSalirJuego != null)
                botonSalirJuego.onClick.AddListener(() => canvasManager.SalirJuego());

            // 7. Volver al Menú Principal a la pantalla de inicio
            if (botonMenuPrincipal != null)
                botonMenuPrincipal.onClick.AddListener(() => canvasManager.CargarEscena("StartMenu"));
        }
    }
}
