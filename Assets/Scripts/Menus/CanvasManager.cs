using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

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
            }

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
