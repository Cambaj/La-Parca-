using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Tecla de Sistema Clasico")]
    [SerializeField] private KeyCode startKey = KeyCode.Return;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape; 

    [Header("References")]
    [SerializeField] private string startScreenScene = "StartScreen";
    [SerializeField] private string menuScene = "Menu";

    [Header("Refencia Directa al Panel")]
    [SerializeField] public GameObject pausePanel;

    public bool isPaused = false;

    private void Awake()
    {
        PlayerPrefs.DeleteKey("PlayerHealth");
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

       if(currentScene == startScreenScene)
        {
            if (Input.GetKeyDown(startKey) || Input.GetKeyDown(KeyCode.JoystickButton7))
            {
                LoadScene(menuScene);
            }
        }
    }
    public void LoadScene(string sceneName, float delay = 2.0f)
    {
        Invoke(nameof(ExecuteLoad), delay);
        activeSceneName = sceneName;
    }
    private string activeSceneName;
    private void ExecuteLoad()
    {
        // Carga la escena de forma Single (borra la anterior automáticamente)
        SceneManager.LoadScene(activeSceneName, LoadSceneMode.Single);
    }
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        // Si la referencia se perdió, buscamos el objeto en la escena aunque esté desactivado
        if (pausePanel == null)
        {
            pausePanel.SetActive(true);

            foreach (Animator anim in pausePanel.GetComponentsInChildren<Animator>())
                anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
        PlayerPauseHandler playerHandler = FindFirstObjectByType<PlayerPauseHandler>();
        if (playerHandler != null) playerHandler.PausePlayer();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        PlayerPauseHandler playerHandler = GetPlayerHandler();
        if (playerHandler != null)
        {
            playerHandler.ResumePlayer();
        }
    }

    public void ReturnToMenuFromPause(float delay = 0f)
    {
        isPaused = false;
        Time.timeScale = 1f;
        LoadScene(menuScene, delay);
    }

    private PlayerPauseHandler GetPlayerHandler()
    {
        return FindFirstObjectByType<PlayerPauseHandler>();
    }

    public void ExitGame()
    {
        Debug.Log("Exit Game...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}

