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

    public static bool CheatInmortalidad { get; private set; }
    public static bool PermisoSaltarEscena { get; private set; }

    [Header("Navegación")]
    [SerializeField] private Button botonVolver;

    // Propiedad estática global para acceso rápido y limpio desde el PlayerMovement
    

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

    private void Update()
    {
        // CONTROL GLOBAL DE CAMBIO DE ESCENAS (Teclado y Mando)
        // Solo funciona si el toggle de "Saltar Niveles" fue activado previamente
        if (PermisoSaltarEscena)
        {
            //  AVANZAR ESCENA: Enter del teclado numérico o Botón 3 del mando (X en Xbox)
            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.JoystickButton3))
            {
                CheatSaltearEscena();
            }

            //  RETROCEDER ESCENA: Tecla Retroceso (Backspace) o Botón 2 del mando (X en PlayStation / Y en Xbox)
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                CheatVolverEscenaAtras();
            }
        }
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

    public void SetCheatInmortal(bool valor)
    {
        if (LevelManager.instance != null) LevelManager.instance.cheatInmortal = valor;
        CheatInmortalidad = valor;

        PlayerPrefs.SetInt("SaveInmortal", valor ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log(" Cheat Inmortal: " + valor);
    }

    public void SetCheatSaltarNiveles(bool valor)
    {
        if (LevelManager.instance != null) LevelManager.instance.cheatSaltarEscenas = valor;
        PermisoSaltarEscena = valor;

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

    public void CheatVolverEscenaAtras()
    {
        if (LevelManager.instance != null && !LevelManager.instance.cheatSaltarEscenas) return;

        int anteriorIndex = SceneManager.GetActiveScene().buildIndex - 1;

        // Protegemos el índice para no intentar cargar escenas negativas fuera de rango
        if (anteriorIndex >= 0)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(anteriorIndex);
            Debug.Log(" [CHEAT ACTIVADO] Regresando a la escena lineal anterior.");
        }
        else
        {
            Debug.LogWarning(" Estás en la escena inicial del build. No se puede ir más atrás.");
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

        bool inmortal = PlayerPrefs.GetInt("SaveInmortal", 0) == 1;
        bool saltar = PlayerPrefs.GetInt("SaveSaltarNiveles", 0) == 1;

        CheatInmortalidad = inmortal;
        PermisoSaltarEscena = saltar;

        if (toggleInmortal != null) toggleInmortal.isOn = inmortal;
        if (toggleSaltarNiveles != null) toggleSaltarNiveles.isOn = saltar;

        if (botonSaltarEscenaDirecto != null)
        {
            botonSaltarEscenaDirecto.gameObject.SetActive(saltar);
        }

        // 3. Sincronizar Trucos con el LevelManager y PlayerPrefs
        if (LevelManager.instance != null)
        {
            LevelManager.instance.cheatInmortal = inmortal;
            LevelManager.instance.cheatSaltarEscenas = saltar;
        }
    }
}
