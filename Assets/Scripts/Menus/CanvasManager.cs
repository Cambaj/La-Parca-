using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;

    [Header("Configuracion del Indicador Visual")]
    [SerializeField] private RectTransform indicadorVisual;
    [SerializeField] private Vector3 offsetIndicador = Vector3.zero;

    private bool isPaused = false;
    private GameObject ultimoSeleccionado;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Time.timeScale = 1f;
        AudioListener.pause = false;
        if (pausePanel != null) pausePanel.SetActive(false);

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    void Update()
    {
        bool escapePressed = false;

#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            escapePressed = true;
        }
#endif

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            escapePressed = true;
        }

        if (escapePressed)
        {
            TogglePause();
        }

        // Si el juego está pausado, rastreamos qué botón tiene el foco del teclado
        if (isPaused && indicadorVisual != null && UnityEngine.EventSystems.EventSystem.current != null)
        {
            GameObject seleccionadoActual = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

            // Si cambiamos de botón con el teclado, movemos el indicador al instante
            if (seleccionadoActual != null && seleccionadoActual != ultimoSeleccionado)
            {
                // Verificar que el objeto seleccionado sea un botón hijo del panel de pausa
                if (seleccionadoActual.transform.IsChildOf(pausePanel.transform))
                {
                    ultimoSeleccionado = seleccionadoActual;
                    RectTransform botonRect = seleccionadoActual.GetComponent<RectTransform>();

                    // Teletransporta las guadañas a la posición exacta del botón actual
                    indicadorVisual.position = botonRect.position + offsetIndicador;
                }

            }
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        PlayerPauseHandler playerHandler = FindFirstObjectByType<PlayerPauseHandler>();

        if (isPaused)
        {
            Time.timeScale = 0f; // Congela el tiempo del juego
            AudioListener.pause = true;

            if (pausePanel != null)
            {
                pausePanel.SetActive(true); // Muestra el panel

                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
                }

                Button primerBoton = pausePanel.GetComponentInChildren<Button>();
                if (primerBoton != null)
                {
                    primerBoton.Select();
                    primerBoton.OnSelect(null);

                    ultimoSeleccionado = primerBoton.gameObject;
                    if (indicadorVisual != null)
                    {
                        indicadorVisual.position = primerBoton.GetComponent<RectTransform>().position + offsetIndicador;
                    }
                }
            }
            if (playerHandler != null) playerHandler.PausePlayer();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f; // Despausa el juego
            AudioListener.pause = false;

            if (pausePanel != null)
            {
                pausePanel.SetActive(false); // Oculta el panel
            }

            if (playerHandler != null) playerHandler.ResumePlayer();
        }
    }

    public void QuitarPausa()
    {
        if (isPaused) TogglePause();
    }

    public void VolverAlMenu(string nombreMenu)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(nombreMenu);
    }
}
