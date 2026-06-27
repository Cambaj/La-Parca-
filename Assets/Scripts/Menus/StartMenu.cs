using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StartMenu : MonoBehaviour
{
    [Header("Configuracion del Boton")]
    [SerializeField] private string sceneName;
    [SerializeField] private bool isExitButton = false;
    [SerializeField] private float delayBeforeLoad = 2.0f;
    // Al usar UnityEvent nativo en el Start, no necesitas configurar el On Click manual
    private void Awake()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(AlHacerClic);
        }
    }

    private void AlHacerClic()
    {

        if (isExitButton)
        {
            if (LevelManager.instance != null) LevelManager.instance.ExitGame();
            return;
        }

        if (LevelManager.instance != null && LevelManager.instance.isPaused && string.IsNullOrEmpty(sceneName))
        {
            LevelManager.instance.ResumeGame();
            return;
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            if (LevelManager.instance != null)
            { 
            LevelManager.instance.LoadScene(sceneName, delayBeforeLoad);
            }
        }
        else
        {
            LevelManager manager = FindFirstObjectByType<LevelManager>();
            if (manager != null) manager.LoadScene(sceneName, delayBeforeLoad);
        }
    }
}
