using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectorNivelesManager : MonoBehaviour
{
    [System.Serializable]
    public struct ReinoConfig
    {
        public int numeroReino;
        public Button[] botonesNiveles;
    }

    [Header("Progreso de Reinos")]
    [SerializeField] private ReinoConfig[] reinos;

    [Header("Indicador Visual (Guadañas)")]
    [SerializeField] private RectTransform indicadorVisual;
    [SerializeField] private Vector3 offsetIndicador = Vector3.zero;

    [Header("Botones Extra Inferiores")]
    [SerializeField] private Button botonVolver;
    [SerializeField] private Button botonBorrarProgreso;

    private GameObject ultimoSeleccionado;
    private Vector3 ultimaPosicionMouse;

    private void Start()
    {
        ActualizarBotonesUI();
        EnfocarPrimerBoton();
        ultimaPosicionMouse = Input.mousePosition;
    }

    private void Update()
    {
        if (indicadorVisual == null || EventSystem.current == null) return;

        // 1. Detección de movimiento real del mouse para evitar interferencias falsas
        bool mouseSeMovio = Vector3.Distance(Input.mousePosition, ultimaPosicionMouse) > 0.1f;
        ultimaPosicionMouse = Input.mousePosition;

        GameObject seleccionadoActual = EventSystem.current.currentSelectedGameObject;

        if (seleccionadoActual == null)
        {
            EnfocarPrimerBoton();
            return;
        }

        // 2. Si el mouse se movió y seleccionó algo por hover, le quitamos prioridad 
        // para que no rompa la navegación fluida de las guadañas con W/S
        if (mouseSeMovio && PointerOverUIObject(seleccionadoActual))
        {
            // Opcional: Si quieres que el mouse sí mueva la guadaña, dejas que pase.
            // Si prefieres bloquearlo para que SOLO use teclado/mando, descomenta la línea de abajo:
            // return; 
        }

        // 3. Mover las guadañas al botón enfocado actualmente
        if (seleccionadoActual != ultimoSeleccionado)
        {
            ultimoSeleccionado = seleccionadoActual;

            RectTransform botonRect = seleccionadoActual.GetComponent<RectTransform>();

            if (botonRect != null)
            {
                Transform padreBoton = botonRect.parent;
                Transform padreIndicador = indicadorVisual.parent;

                // Cálculo matemático exacto relativo al lienzo principal
                Vector3 posicionFinal = padreIndicador.InverseTransformPoint(padreBoton.TransformPoint(botonRect.localPosition));
                indicadorVisual.localPosition = posicionFinal + offsetIndicador;
            }
        }
    }

    private bool PointerOverUIObject(GameObject obj)
    {
        return EventSystem.current.currentSelectedGameObject == obj;
    }

    private void ActualizarBotonesUI()
    {
        if (reinos == null || reinos.Length == 0) return;

        foreach (var reino in reinos)
        {
            for (int i = 0; i < reino.botonesNiveles.Length; i++)
            {
                if (reino.botonesNiveles[i] == null) continue;

                string clave = "Nivel_" + reino.numeroReino + "-" + (i + 1);
                int estadoDesbloqueado = PlayerPrefs.GetInt(clave, 0);

                // El Reino 1 Nivel 1 (Muerte 1) siempre se puede jugar
                if (reino.numeroReino == 1 && i == 0)
                {
                    reino.botonesNiveles[i].interactable = true;
                }
                else
                {
                    reino.botonesNiveles[i].interactable = (estadoDesbloqueado == 1);
                }
            }
        }
    }

    private void EnfocarPrimerBoton()
    {
        if (reinos == null || reinos.Length == 0) return;

        foreach (var reino in reinos)
        {
            if (reino.numeroReino == 1 && reino.botonesNiveles.Length > 0 && reino.botonesNiveles[0] != null)
            {
                reino.botonesNiveles[0].Select();
                break;
            }
        }
    }
    public void ResetearTodoElProgreso()
    {
        // Borra todos los PlayerPrefs del juego
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("¡Progreso eliminado por completo!");

        // Refresca visualmente los botones para que se bloqueen todos de inmediato
        ActualizarBotonesUI();

        // Devolvemos el foco de navegación al primer nivel por seguridad
        EnfocarPrimerBoton();
    }

}
