using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    // Variables globales de trucos (Cheats)
    public bool cheatInmortal = false;
    public bool cheatSaltarEscenas = false;

    // Nombres de tus escenas base
    public string escenaMenu = "MenuPrincipal";
    public string escenaSelector = "SelectorNiveles";

    private void Awake()
    {
        // Asegura que solo exista un LevelManager en todo el juego
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

        // Forzar que el nivel 1-1 siempre esté desbloqueado la primera vez
        if (!PlayerPrefs.HasKey("Nivel_1-1"))
        {
            PlayerPrefs.SetInt("Nivel_1-1", 1); // 1 = Desbloqueado, 0 = Bloqueado
            PlayerPrefs.Save();
        }
    }

    public void DesbloquearNivel(int reino, int nivel)
    {
        string clave = "Nivel_" + reino + "-" + nivel;
        PlayerPrefs.SetInt(clave, 1);
        PlayerPrefs.Save();
    }

    public void CargarEscena(string nombreEscena)
    {
        Time.timeScale = 1f; // Asegura que el tiempo corra al cambiar de pantalla
        AudioListener.pause = false;
        SceneManager.LoadScene(nombreEscena);
    }

    public void SalirDelJuego()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}

