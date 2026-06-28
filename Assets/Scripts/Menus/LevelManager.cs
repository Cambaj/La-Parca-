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

                if (i == 0)
                {
                    reino.botonesNiveles[i].interactable = true;
                    continue;
                }

                // Generamos la clave única de guardado, ej: "Hambruna_Nivel_2"
                string claveGuardado = reino.nombreReino + "_Nivel_" + (i + 1);

                int nivelDesbloqueado = PlayerPrefs.GetInt(claveGuardado, 0);

                reino.botonesNiveles[i].interactable = (nivelDesbloqueado == 1);
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
        if (reinos != null)
        {
            foreach (var reino in reinos)
            {
                for (int i = 2; i <= reino.botonesNiveles.Length; i++)
                {
                    PlayerPrefs.DeleteKey(reino.nombreReino + "_Nivel_" + i);
                }
            }
        }
        PlayerPrefs.DeleteAll();
        Debug.Log("Progreso de niveles reseteados.");
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

