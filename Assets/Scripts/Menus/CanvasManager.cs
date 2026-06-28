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

    [Header("Configuracion del Indicador Visual")]
    [SerializeField] private RectTransform indicadorVisual;
    [SerializeField] private Vector3 offsetIndicador = Vector3.zero;

    private bool isPaused = false;
    private GameObject ultimoSeleccionado;
    private GameObject panelActivoActual;
    private GameObject panelAnterior;

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

     void Start() => AsegurarFocoInicial();
     
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

            if (seleccionadoActual != null && seleccionadoActual != ultimoSeleccionado)
            {
                // Si el jugador está tocando un Slider (barra de volumen), ocultamos temporalmente las guadañas o las dejamos fijas
                if (seleccionadoActual.GetComponent<Slider>() != null)
                {
                    // Opcional: puedes ajustar una posición fija para los Sliders
                }

                ultimoSeleccionado = seleccionadoActual;
                RectTransform botonRect = seleccionadoActual.GetComponent<RectTransform>();
                if (botonRect != null)
                {
                    indicadorVisual.position = botonRect.position + offsetIndicador;
                }
            }
        }
    }

    private void AsegurarFocoInicial()
    {
        if (mainMenuPanel != null && mainMenuPanel.activeSelf) panelActivoActual = mainMenuPanel;
        else if (levelSelectorPanel != null && levelSelectorPanel.activeSelf) panelActivoActual = levelSelectorPanel;
        else if (pausePanel != null && pausePanel.activeSelf) panelActivoActual = pausePanel;
        else if (pausePanel != null && pausePanel.activeSelf) panelActivoActual = pausePanel;

        if (panelActivoActual != null)
        {
            // Busca primero un botón, si no, busca un Slider para darle el foco
            Selectable primerElemento = panelActivoActual.GetComponentInChildren<Button>();
            if (primerElemento == null) primerElemento = panelActivoActual.GetComponentInChildren<Slider>();

            if (primerElemento != null)
            {
                primerElemento.Select();
                if (indicadorVisual != null)
                {
                    indicadorVisual.position = primerElemento.GetComponent<RectTransform>().position + offsetIndicador;
                }
            }
        }
    }

    // Flujo de Navegacion 

    public void CambiarPanel(GameObject panelDestino)
    {
        if (panelActivoActual != optionsPanel) panelAnterior = panelActivoActual;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectorPanel != null) levelSelectorPanel.SetActive(false);
        if(optionsPanel != null) optionsPanel.SetActive(false);
        if (optionsPanel != null) pausePanel.SetActive(false);

        panelDestino.SetActive(true);
        panelActivoActual = panelDestino;
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
            Time.timeScale = 0f; // Congela el tiempo del juego
            AudioListener.pause = true;
            if (pausePanel != null) pausePanel.SetActive(true); // Muestra el panel
            panelActivoActual = pausePanel;
            AsegurarFocoInicial();
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
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if(playerHandler !=null) playerHandler.PausePlayer();
        }
    }

    //Acciones de juego y reinicio 

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
        Time.timeScale += 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(nombreEscena);
    }

    public void SalirJuego()
    {
        Application.Quit();
    }

    //Ajustes de volumen

    public void CambiarVolumenMusica(float valor)
    {
        Debug.Log("Volumen Musica" + valor);
    }

    public void CambiarVolumenSonido(float valor)
    {
        Debug.Log("Volumen Sonido:" + valor);
    }
}
