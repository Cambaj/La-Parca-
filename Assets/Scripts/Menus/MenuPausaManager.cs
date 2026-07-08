using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPausaManager : MonoBehaviour
{
    [Header("Paneles de la UI")]
    [SerializeField] private GameObject panelPausa;

    [Header("Indicador Visual (Guadañas)")]
    [SerializeField] private RectTransform indicadorVisual;
    [SerializeField] private Vector3 offsetIndicador = Vector3.zero;

    [Header("Primer Botón Seleccionado")]
    [SerializeField] private Button botonReanudar;

    private GameObject ultimoSeleccionado;
    private bool juegoPausado = false;

    private void Start()
    {
        // Asegurarnos de que el menú empiece oculto y el tiempo corra normal
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Escuchar el input de Pausa (Escape o la tecla que definas)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }

        // Si el menú no está activo, no calculamos focos ni guadañas
        if (!juegoPausado || indicadorVisual == null || EventSystem.current == null) return;

        GameObject seleccionadoActual = EventSystem.current.currentSelectedGameObject;

        // Blindaje contra clics fantasmas del mouse en el fondo
        if (seleccionadoActual == null)
        {
            if (ultimoSeleccionado != null && ultimoSeleccionado.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(ultimoSeleccionado);
            }
            return;
        }

        // Mover las guadañas fluidamente entre las opciones de pausa
        if (seleccionadoActual != ultimoSeleccionado)
        {
            RectTransform elementoRect = seleccionadoActual.GetComponent<RectTransform>();
            if (elementoRect != null && elementoRect.parent != null)
            {
                ultimoSeleccionado = seleccionadoActual;

                Transform padreElemento = elementoRect.parent;
                Transform padreIndicador = indicadorVisual.parent;

                if (padreIndicador != null)
                {
                    Vector3 posicionFinal = padreIndicador.InverseTransformPoint(padreElemento.TransformPoint(elementoRect.localPosition));
                    indicadorVisual.localPosition = posicionFinal + offsetIndicador;
                }
            }
        }
    }

    // ==========================================
    // MÉTODOS DE LOGICA DE BOTONES
    // ==========================================

    public void PausarJuego()
    {
        juegoPausado = true;
        panelPausa.SetActive(true);
        Time.timeScale = 0f; // Congela físicas, animaciones y movimientos del juego

        // Forzar al teclado a tomar el control en el botón superior
        if (botonReanudar != null)
        {
            botonReanudar.Select();
            ultimoSeleccionado = botonReanudar.gameObject;
        }
    }

    public void ReanudarJuego()
    {
        juegoPausado = false;
        panelPausa.SetActive(false);
        Time.timeScale = 1f; // Devuelve el tiempo a la normalidad
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f; // ¡CRUCIAL! Si no restauras el tiempo antes de cargar, la escena se congela
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void IrASeleccionNiveles()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SelectorNiveles"); // Asegúrate de escribir el nombre exacto de tu escena
    }

    public void IrAMenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void SalirDelJuego()
    {
        Debug.Log(" Saliendo del juego...");
        Application.Quit();
    }
    public void ReiniciarReino()
    {
        Time.timeScale = 1f; // Restauramos el tiempo del juego

        // Aquí le pedimos al LevelManager global que nos diga cuál es el nivel inicial del reino actual
        if (LevelManager.instance != null)
        {
            // Supongamos que tu LevelManager maneja una función o variable con el nombre de la escena inicial
            string escenaPrimerNivel = LevelManager.instance.ObtenerPrimerNivelDelReinoActual();
            SceneManager.LoadScene(escenaPrimerNivel);
        }
        else
        {
            // Opción B (Temporal): Si no tienes esa lógica en el LevelManager, 
            // puedes hacer que por defecto regrese al Selector de Niveles para que elija desde dónde reiniciar.
            SceneManager.LoadScene("SelectorNiveles");
        }

    }

}
