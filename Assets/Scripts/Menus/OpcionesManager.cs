using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpcionesManager : MonoBehaviour
{
    [Header("Indicador Visual (Guadañas)")]
    [SerializeField] private RectTransform indicadorVisual;
    [SerializeField] private Vector3 offsetIndicador = Vector3.zero;

    [Header("Controles de Audio")]
    [SerializeField] private AudioMixer audioMixer; //  Arrastrá tu MasterMixer aquí
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    [Header("Trucos (Cheats)")]
    [SerializeField] private Toggle toggleInmortal;
    [SerializeField] private Toggle toggleSaltarNiveles;
    [SerializeField] private Button botonSaltarEscenaDirecto; //  Arrastrá el botón físico para saltear escena aquí

    [Header("Navegación")]
    [SerializeField] private Button botonVolver;

    // Propiedad estática global para acceso rápido y limpio desde el PlayerMovement
    public static bool CheatInmortalidad { get; private set; }

    private void Start()
    {
        // 1. Ocultar por completo las guadañas al iniciar el menú
        if (indicadorVisual != null)
        {
            indicadorVisual.gameObject.SetActive(false);
        }

        // 2. Cargar configuraciones de Audio y Persistencia
        CargarConfiguracionesAudioYCheats();

        // 3. Escuchar los cambios de los Toggles en tiempo real
        if (toggleInmortal != null) toggleInmortal.onValueChanged.AddListener(SetCheatInmortal);
        if (toggleSaltarNiveles != null) toggleSaltarNiveles.onValueChanged.AddListener(SetCheatSaltarNiveles);

        // 4. Escuchar los cambios de los Sliders en tiempo real por código
        if (sliderMusica != null) sliderMusica.onValueChanged.AddListener(SetVolumenMusica);
        if (sliderSFX != null) sliderSFX.onValueChanged.AddListener(SetVolumenSFX);

        // 5. Escuchar el clic del botón de saltear escena
        if (botonSaltarEscenaDirecto != null) botonSaltarEscenaDirecto.onClick.AddListener(CheatSaltearEscena);
    }

    // ==========================================
    //  SECCIÓN DE CONTROL DE AUDIO (MIXER)
    // ==========================================

    public void SetVolumenMusica(float valorSlider)
    {
        // Conversión matemática logarítmica para el AudioMixer (evita el 0 absoluto)
        float decibelios = Mathf.Log10(Mathf.Max(valorSlider, 0.0001f)) * 20f;
        if (audioMixer != null)
        {
            audioMixer.SetFloat("VolMusica", decibelios);
        }

        PlayerPrefs.SetFloat("SaveMusica", valorSlider);
        PlayerPrefs.Save();
    }

    public void SetVolumenSFX(float valorSlider)
    {
        float decibelios = Mathf.Log10(Mathf.Max(valorSlider, 0.0001f)) * 20f;
        if (audioMixer != null)
        {
            audioMixer.SetFloat("VolSFX", decibelios);
        }

        PlayerPrefs.SetFloat("SaveSFX", valorSlider);
        PlayerPrefs.Save();
    }

    // ==========================================
    // MÉTODOS PÚBLICOS PARA EL EVENT TRIGGER (Guadañas)
    // ==========================================

    public void MostrarGuadañasEnVolver()
    {
        if (indicadorVisual == null || botonVolver == null) return;

        RectTransform botonRect = botonVolver.GetComponent<RectTransform>();
        if (botonRect != null && botonRect.parent != null)
        {
            indicadorVisual.gameObject.SetActive(true);

            Transform padreBoton = botonRect.parent;
            Transform padreIndicador = indicadorVisual.parent;

            if (padreIndicador != null)
            {
                Vector3 posicionFinal = padreIndicador.InverseTransformPoint(padreBoton.TransformPoint(botonRect.localPosition));
                indicadorVisual.localPosition = posicionFinal + offsetIndicador;
            }
        }
    }

    public void OcultarGuadañas()
    {
        if (indicadorVisual != null)
        {
            indicadorVisual.gameObject.SetActive(false);
        }
    }

    // ==========================================
    //  SECCIÓN DE TRUCOS (CHEATS)
    // ==========================================

    private void SetCheatInmortal(bool valor)
    {
        if (LevelManager.instance != null) LevelManager.instance.cheatInmortal = valor;
        CheatInmortalidad = valor;

        PlayerPrefs.SetInt("SaveInmortal", valor ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log(" Cheat Inmortal: " + valor);
    }

    private void SetCheatSaltarNiveles(bool valor)
    {
        if (LevelManager.instance != null) LevelManager.instance.cheatSaltarEscenas = valor;

        // Prendemos o apagamos el botón físico de saltear según el Toggle general de trucos
        if (botonSaltarEscenaDirecto != null)
        {
            botonSaltarEscenaDirecto.gameObject.SetActive(valor);
        }

        PlayerPrefs.SetInt("SaveSaltarNiveles", valor ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log(" Cheat Saltar Niveles (Permiso): " + valor);
    }

    public void CheatSaltearEscena()
    {
        // Si el truco general está apagado en el LevelManager, bloqueamos la acción por seguridad
        if (LevelManager.instance != null && !LevelManager.instance.cheatSaltarEscenas) return;

        int siguienteIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (siguienteIndex < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f; // Asegurar que el tiempo corra normal si venimos de una pausa
            SceneManager.LoadScene(siguienteIndex);
            Debug.Log(" [CHEAT ACTIVADO] Saltando a la siguiente escena lineal.");
        }
        else
        {
            Debug.LogWarning(" No hay más escenas por delante en los Build Settings.");
        }
    }

    // ==========================================
    //  CARGA Y PERSISTENCIA DE CONFIGURACIONES
    // ==========================================

    private void CargarConfiguracionesAudioYCheats()
    {
        // 1. Cargar volumen de Música (por defecto al máximo = 1)
        float musicaGuardada = PlayerPrefs.GetFloat("SaveMusica", 1f);
        if (sliderMusica != null) sliderMusica.value = musicaGuardada;
        SetVolumenMusica(musicaGuardada);

        // 2. Cargar volumen de SFX (por defecto al máximo = 1)
        float sfxGuardado = PlayerPrefs.GetFloat("SaveSFX", 1f);
        if (sliderSFX != null) sliderSFX.value = sfxGuardado;
        SetVolumenSFX(sfxGuardado);

        // 3. Sincronizar Trucos con el LevelManager y PlayerPrefs
        if (LevelManager.instance != null)
        {
            // Cargamos lo que el archivo recuerde o lo que ya tenga el LevelManager
            bool inmortal = PlayerPrefs.GetInt("SaveInmortal", LevelManager.instance.cheatInmortal ? 1 : 0) == 1;
            bool saltar = PlayerPrefs.GetInt("SaveSaltarNiveles", LevelManager.instance.cheatSaltarEscenas ? 1 : 0) == 1;

            LevelManager.instance.cheatInmortal = inmortal;
            LevelManager.instance.cheatSaltarEscenas = saltar;

            if (toggleInmortal != null) toggleInmortal.isOn = inmortal;
            if (toggleSaltarNiveles != null) toggleSaltarNiveles.isOn = saltar;

            CheatInmortalidad = inmortal;

            // Mostrar u ocultar el botón de salto directo al arrancar la escena de opciones
            if (botonSaltarEscenaDirecto != null)
            {
                botonSaltarEscenaDirecto.gameObject.SetActive(saltar);
            }
        }
    }
}
