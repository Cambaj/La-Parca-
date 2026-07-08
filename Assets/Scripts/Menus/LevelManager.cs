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

    public string ObtenerPrimerNivelDelReinoActual()
    {
        // Obtiene el nombre de la escena que se está jugando ahora mismo (ej: "Nivel_1-3")
        string nombreEscenaActual = SceneManager.GetActiveScene().name;

        // Buscamos si el nombre sigue el patrón "Nivel_"
        if (nombreEscenaActual.Contains("Nivel_"))
        {
            // Cortamos el nombre para obtener el número de reino. 
            // Si la escena es "Nivel_1-3", queremos extraer el "1"
            string[] partes = nombreEscenaActual.Split('_'); // Separa en ["Nivel", "1-3"]
            if (partes.Length > 1)
            {
                string[] subPartes = partes[1].Split('-'); // Separa en ["1", "3"]
                string numeroReino = subPartes[0]; // Nos quedamos con el "1"

                // Devolvemos el nombre del primer nivel de ese reino (ej: "Nivel_1-1")
                string escenaPrimerNivel = "Nivel_" + numeroReino + "-1";
                return escenaPrimerNivel;
            }
        }

        // Si estás en una escena con un nombre raro o fuera de la estructura, 
        // te devuelve al selector por seguridad para no romper el juego
        return "SelectorNiveles";

    }

}

