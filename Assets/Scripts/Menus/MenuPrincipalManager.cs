using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuPrincipalManager : MonoBehaviour
{
    [Header("Indicador Visual (Guadañas)")]
    [SerializeField] private RectTransform indicadorVisual;
    [SerializeField] private Vector3 offsetIndicador = Vector3.zero;

    [Header("Primer Botón (Jugar)")]
    [SerializeField] private Button botonInicial;

    private GameObject ultimoSeleccionado;

    private void Start()
    {
        // Forzamos el foco inicial en el botón de Jugar al empezar
        EnfocarBotonInicial();
    }

    private void Update()
    {
        // 1. Si el EventSystem no está listo, salimos
        if (EventSystem.current == null || indicadorVisual == null) return;

        GameObject seleccionadoActual = EventSystem.current.currentSelectedGameObject;

        // 2. Si el mouse deseleccionó el botón haciendo clic afuera...
        if (seleccionadoActual == null)
        {
            if (ultimoSeleccionado != null && ultimoSeleccionado.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(ultimoSeleccionado);
            }
            else
            {
                EnfocarBotonInicial();
            }
            return;
        }

        // 3. Si el botón seleccionado cambió en este frame
        if (seleccionadoActual != ultimoSeleccionado)
        {
            // Intentamos obtener el componente RectTransform de manera segura
            RectTransform botonRect = seleccionadoActual.GetComponent<RectTransform>();

            // ¡SOLUCIÓN! Solo procesamos el movimiento si el objeto realmente tiene un RectTransform y un padre válido
            if (botonRect != null && botonRect.parent != null)
            {
                ultimoSeleccionado = seleccionadoActual; // Solo actualizamos si es un botón válido

                Transform padreBoton = botonRect.parent;
                Transform padreIndicador = indicadorVisual.parent;

                if (padreIndicador != null)
                {
                    // Cálculo matemático exacto para posicionar las guadañas
                    Vector3 posicionFinal = padreIndicador.InverseTransformPoint(padreBoton.TransformPoint(botonRect.localPosition));
                    indicadorVisual.localPosition = posicionFinal + offsetIndicador;
                }
            }
            else
            {
                // Si el mouse seleccionó algo raro que no es un botón de la UI, restauramos el último botón válido
                if (ultimoSeleccionado != null)
                {
                    EventSystem.current.SetSelectedGameObject(ultimoSeleccionado);
                }
            }
        }
    }

    private void EnfocarBotonInicial()
    {
        if (botonInicial != null)
        {
            botonInicial.Select();
            ultimoSeleccionado = botonInicial.gameObject;
        }
    }
}
