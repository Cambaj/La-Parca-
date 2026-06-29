using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;


public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [Header("Tecla de Sistema Clasico")]
    [SerializeField] private KeyCode startKey = KeyCode.Return;

    [Header("References")]
    [SerializeField] private string startScreenScene = "StartScreen";
    [SerializeField] private string menuScene = "Menu";

    private string activeSceneName;

    [System.Serializable]
    public struct ReinoBotones
    {
        public string nombreReino;
        public Button[] botonesNiveles;
    }

    [Header("Progreso de Reinos y Niveles")]
    [SerializeField] private ReinoBotones[] reinos;

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

    private void Start()
    {
        // 1. Primero bloqueamos ABSOLUTAMENTE TODOS los botones de todos los reinos
        for (int r = 0; r < reinos.Length; r++)
        {
            for (int n = 0; n < reinos[r].botonesNiveles.Length; n++)
            {
                if (reinos[r].botonesNiveles[n] != null)
                {
                    reinos[r].botonesNiveles[n].interactable = false;
                }
            }
        }
        // 2. Cargamos el progreso guardado o inicializamos por primera vez
        // El Reino 0 (Hambruna) Nivel 1 SIEMPRE debe estar disponible para empezar
        PlayerPrefs.SetInt("Progreso_Hambruna", Mathf.Max(PlayerPrefs.GetInt("Progreso_Hambruna", 1), 1));

        // 3. Aplicamos el progreso real que el jugador tenga guardado en el disco
        ActualizarSelectorNiveles();
    }

    private void Update()
    {
        string currentScene = SceneManager.GetActiveScene().name;

       if(currentScene == startScreenScene)
        {
            if (Input.GetKeyDown(startKey) || Input.GetKeyDown(KeyCode.JoystickButton7))
            {
                LoadScene(menuScene, 0f);
            }
        }
    }

    public void ActualizarSelectorNiveles()
    {
        if (reinos == null || reinos.Length == 0) return;

        foreach (var reino in reinos)
        {
            for (int i = 0; i < reino.botonesNiveles.Length; i++)
            {
                if (reino.botonesNiveles[i] == null) continue;

                string claveGuardado = reino.nombreReino + "_Nivel_" + (i + 1);

                int nivelDesbloqueado = PlayerPrefs.GetInt(claveGuardado, 0);

                if (i == 0 && reino.nombreReino == "Muerte")
                {
                    reino.botonesNiveles[i].interactable = true;

                }
                else 
                {
                    reino.botonesNiveles[i].interactable = (nivelDesbloqueado == 1);
                }
            }
        }
    }

    public void DesbloquearSiguienteNivel(string nombreReino, int numeroNivelAlLiberar)
    {
        string claveGuardado = nombreReino + "_Nivel_" + numeroNivelAlLiberar;
        PlayerPrefs.SetInt(claveGuardado, 1);
        PlayerPrefs.Save();

        ActualizarSelectorNiveles();
    }

    [ContextMenu("Borrar Todo el Progreso")]
    public void BorrarProgreso()
    {
        PlayerPrefs.DeleteAll();
        ActualizarSelectorNiveles();
    }

    // FUNCIONEES NATIVAS DE ESCENA Y PAUSA

    public void LoadScene(string sceneName, float delay = 2.0f)
    {
        Invoke(nameof(ExecuteLoad), delay);
        activeSceneName = sceneName;
    }
    private void ExecuteLoad()
    {
        // Carga la escena de forma Single (borra la anterior automáticamente)
        SceneManager.LoadScene(activeSceneName, LoadSceneMode.Single);
    }
    
}

