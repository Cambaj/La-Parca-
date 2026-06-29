using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.Android.AndroidGame;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectorPanel;
    [SerializeField] private GameObject optionsPanel;

    public GameObject OptionsPanel => optionsPanel;

    [Header("Configuracion del Indicador Visual")]
    [SerializeField] private RectTransform indicadorVisual;
    [SerializeField] private Vector3 offsetIndicador = Vector3.zero;

    private bool isPaused = false;
    private GameObject ultimoSeleccionado;
    private GameObject panelActivoActual;
    private GameObject panelAnterior;

    //Variables Globales para trucos 
    public static bool CheatInmortal = false;
    public static bool CheatSaltarEscenas = false;

    [Header("Componente de Opciones")]
    [SerializeField] private Toggle toggleInmortal;
    [SerializeField] private Toggle toggleSaltar;


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
        if (optionsPanel != null) optionsPanel.SetActive(false);

        AsegurarFocoInicial();
    }

    private void Start()
    {
        // Le dice al LevelManager que refresque los botones que tenga asignados
        if (LevelManager.instance != null)
        {
            LevelManager.instance.ActualizarSelectorNiveles();
        }
        AsegurarFocoInicial();
    }
    void Update()
    {
        if (pausePanel != null)
        {
            bool escapePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (UnityEngine.InputSystem.Keyboard.current != null &&
                UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
                escapePressed = true;
#endif

            if (Input.GetKeyDown(KeyCode.Escape)) escapePressed = true;
            if (escapePressed) TogglePause();
        }

        // Sistema de movimiento automático de las guadañas
        if (indicadorVisual != null && UnityEngine.EventSystems.EventSystem.current != null)
        {
            GameObject seleccionadoActual = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

            if (seleccionadoActual == null && panelActivoActual == levelSelectorPanel)
            {
                AsegurarFocoInicial();
                return;
            }

            if (seleccionadoActual != null && seleccionadoActual != ultimoSeleccionado)
            {
                ultimoSeleccionado = seleccionadoActual;
                RectTransform botonRect = seleccionadoActual.GetComponent<RectTransform>();
                if (botonRect != null)
                {
                    indicadorVisual.position = botonRect.position + offsetIndicador;
                }
            }
        }

        // Adentro del Update() actual, abajo de todo:
        if (CheatSaltarEscenas && Input.GetKeyDown(KeyCode.N))
        {
            int siguienteEscenaIndex = SceneManager.GetActiveScene().buildIndex + 1;
            // Si no es la última escena del juego, avanza
            if (siguienteEscenaIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(siguienteEscenaIndex);
            }
        }

    }

    private void AsegurarFocoInicial()
    {
        if (mainMenuPanel != null && mainMenuPanel.activeSelf) panelActivoActual = mainMenuPanel;
        else if (levelSelectorPanel != null && levelSelectorPanel.activeSelf) panelActivoActual = levelSelectorPanel;
        else if (pausePanel != null && pausePanel.activeSelf) panelActivoActual = pausePanel;
        else panelActivoActual = null;

        if (panelActivoActual != null)
        {
            Selectable elementoASeleccionar = null;

            // Si estamos en el selector de niveles, buscamos el primer botón que SÍ sea interactuable
            if (panelActivoActual == levelSelectorPanel)
            {
                Selectable[] todosLosBotones = panelActivoActual.GetComponentsInChildren<Selectable>();
                foreach (Selectable s in todosLosBotones)
                {
                    // Buscamos el primero de la lista que no esté bloqueado (en tu caso, Muerte Nivel 1)
                    if (s.interactable && (s is UnityEngine.UI.Button || s is UnityEngine.UI.Slider))
                    {
                        elementoASeleccionar = s;
                        break; // Corta la búsqueda apenas encuentra tu nivel activo
                    }
                }
            }
            else
            {
                // Para el menú principal o pausa, usa el comportamiento estándar
                elementoASeleccionar = panelActivoActual.GetComponentInChildren<UnityEngine.UI.Button>();
            }

            // Si encontramos el botón activo, le clavamos el foco y movemos las guadañas ahí
            if (elementoASeleccionar != null)
            {
                elementoASeleccionar.Select();
                if (indicadorVisual != null)
                {
                    // Forzamos al indicador a moverse instantáneamente a la posición del botón
                    indicadorVisual.position = elementoASeleccionar.GetComponent<RectTransform>().position + offsetIndicador;
                }
            }
        }
    }

    public void UI_DesbloquearNivelDev(string reino)
    {
        if (LevelManager.instance != null) LevelManager.instance.DesbloquearSiguienteNivel(reino, 2);
    }

    public void U_BorrarProgresoDev()
    {
        if (LevelManager.instance != null) LevelManager.instance.BorrarProgreso();
        AsegurarFocoInicial();
    }

    public void CambiarPanel(GameObject panelDestino)
    {
        if (panelActivoActual != optionsPanel) panelAnterior = panelActivoActual;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectorPanel != null) levelSelectorPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        panelDestino.SetActive(true);
        panelActivoActual = panelDestino;
        if (panelDestino == levelSelectorPanel && LevelManager.instance != null)
        {
            LevelManager.instance.ActualizarSelectorNiveles();
        }
        AsegurarFocoInicial();
    }

    public void VolverAtras()
    {
        if (panelAnterior != null) CambiarPanel(panelAnterior);
        else if (mainMenuPanel != null) CambiarPanel(mainMenuPanel);
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        PlayerPauseHandler playerHandler = FindFirstObjectByType<PlayerPauseHandler>();

        if (isPaused)
        {
            Time.timeScale = 0f;
            AudioListener.pause = true;
            if (pausePanel != null) pausePanel.SetActive(true);

            if (playerHandler != null) playerHandler.PausePlayer();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (playerHandler != null) playerHandler.ResumePlayer();
        }
    }

    public void ReiniciarNivelActual()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReiniciarReino(string nombrePrimerNivelDelReino)
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(nombrePrimerNivelDelReino);
    }

    public void CargarEscena(string nombreEscena)
    {
        Time.timeScale = 1f; // Corregido el acelerador involuntario de tiempo
        AudioListener.pause = false;
        SceneManager.LoadScene(nombreEscena);
    }

    public void SalirJuego()
    {
        Debug.Log("Intentando salir del juego...");

        // Si estás jugando dentro del Editor de Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // Si el juego ya está compilado en un .exe ejecutable
            Application.Quit();
#endif
    }

    // --- NUEVO: Lógica de Cheats (Se vinculan a los Toggles) ---

    public void UI_ToggleInmortalidad(bool estado)
    {
        CheatInmortal = estado;
        Debug.Log("Modo Dios: " + (CheatInmortal ? "ACTIVADO" : "DESACTIVADO"));
    }

    public void UI_ToggleSaltarEscenas(bool estado)
    {
        CheatSaltarEscenas = estado;
        Debug.Log("Saltear Escenas: " + (CheatSaltarEscenas ? "ACTIVADO" : "DESACTIVADO"));
    }

    // --- AJUSTES DE VOLUMEN (Actualizados) ---

    public void CambiarVolumenMusica(float valor)
    {
        // El valor va de 0.0f a 1.0f. Aquí conectarías con tu AudioMixer a futuro
        Debug.Log("Volumen Musica: " + (valor * 100).ToString("F0") + "%");
    }

    public void CambiarVolumenSonido(float valor)
    {
        Debug.Log("Volumen Sonido: " + (valor * 100).ToString("F0") + "%");
    }


}
