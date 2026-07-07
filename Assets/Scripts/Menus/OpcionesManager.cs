using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OpcionesManager : MonoBehaviour
{
    [Header("Indicador Visual (Guadañas)")]
    [SerializeField] private RectTransform indicadorVisual;
    [SerializeField] private Vector3 offsetIndicador = Vector3.zero;

    [Header("Controles de Audio")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSFX;

    [Header("Trucos (Cheats)")]
    [SerializeField] private Toggle toggleInmortal;
    [SerializeField] private Toggle toggleSaltarNiveles;

    [Header("Navegación")]
    [SerializeField] private Button botonVolver;

    private GameObject ultimoSeleccionado;
    private Vector3 ultimaPosicionMouse;
    private bool usandoMouse = false;

    private void Start()
    {
        if (LevelManager.instance != null)
        {
            toggleInmortal.isOn = LevelManager.instance.cheatInmortal;
            toggleSaltarNiveles.isOn = LevelManager.instance.cheatSaltarEscenas;
        }

        toggleInmortal.onValueChanged.AddListener(SetCheatInmortal);
        toggleSaltarNiveles.onValueChanged.AddListener(SetCheatSaltarNiveles);

        if (sliderMusica != null)
        {
            sliderMusica.Select();
            ultimoSeleccionado = sliderMusica.gameObject;
        }

        // Guardamos la posición inicial del mouse
        ultimaPosicionMouse = Input.mousePosition;
    }

    private void Update()
    {
        if (indicadorVisual == null || EventSystem.current == null) return;

        GameObject seleccionadoActual = EventSystem.current.currentSelectedGameObject;

        // 1. Si el mouse hace clic en el fondo, NO forzamos el foco inmediatamente.
        // Solo restauramos la selección si el jugador presiona activamente el teclado/mando.
        if (seleccionadoActual == null)
        {
            if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
            {
                if (ultimoSeleccionado != null && ultimoSeleccionado.activeInHierarchy)
                {
                    EventSystem.current.SetSelectedGameObject(ultimoSeleccionado);
                }
                else if (sliderMusica != null)
                {
                    sliderMusica.Select();
                }
            }
            return;
        }

        // 2. Si hay un objeto seleccionado y es distinto al último, movemos las guadañas
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

    private void SetCheatInmortal(bool valor)
    {
        if (LevelManager.instance != null) LevelManager.instance.cheatInmortal = valor;
        Debug.Log(" Cheat Inmortal: " + valor);
    }

    private void SetCheatSaltarNiveles(bool valor)
    {
        if (LevelManager.instance != null) LevelManager.instance.cheatSaltarEscenas = valor;
        Debug.Log(" Cheat Saltar Niveles: " + valor);
    }
}
