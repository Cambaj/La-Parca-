using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectorNivelesManager : MonoBehaviour
{
    [Header("Primer Botón")]
    [SerializeField] private Button botonNivel1;

    [System.Serializable]
    public struct ReinoConfig
    {
        public int numeroReino;
        public Button[] botonesNiveles;
    }

    [Header("Progreso de Reinos")]
    [SerializeField] private ReinoConfig[] reinos;

    [Header("Botones Extra Inferiores")]
    [SerializeField] private Button botonVolver;
    [SerializeField] private Button botonBorrarProgreso;

    private System.Collections.IEnumerator Start()
    {
        // 1. Cargar el progreso lógico de los niveles (Desbloquear / Bloquear)
        ActualizarBotonesUI();

        // 2. Limpiar cualquier selección residual en el EventSystem
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // 3. ESPERAR UN FRAME: Esto le da tiempo a Unity para renderizar la UI 
        // y que el foco inicial responda perfectamente.
        yield return new WaitForEndOfFrame();

        // 4. Forzar la selección del primer nivel para que inicie resaltado en amarillo
        if (botonNivel1 != null)
        {
            botonNivel1.Select();
        }
        else
        {
            EnfocarPrimerBoton();
        }
    }

    private void ActualizarBotonesUI()
    {
        if (reinos == null || reinos.Length == 0) return;

        foreach (var reino in reinos)
        {
            for (int i = 0; i < reino.botonesNiveles.Length; i++)
            {
                if (reino.botonesNiveles[i] == null) continue;

                // Nomenclatura dinámica perfecta: Nivel_1-1, Nivel_1-2...
                string clave = "Nivel_" + reino.numeroReino + "-" + (i + 1);

                // Si el truco de saltear escenas está activo en el OpcionesManager, forzamos que devuelva 1 (Desbloqueado)
                int estadoDesbloqueado = OpcionesManager.PermisoSaltarEscena ? 1 : PlayerPrefs.GetInt(clave, 0);

                bool puedeJugar = false;

                // El Reino 1 Nivel 1 siempre se puede jugar de forma nativa
                if (reino.numeroReino == 1 && i == 0)
                {
                    puedeJugar = true;
                }
                else
                {
                    puedeJugar = (estadoDesbloqueado == 1);
                }

                // .interactable = false aplica automáticamente tu "Disabled Sprite" (Bloqueado)
                // .interactable = true habilita el camino para que use tus estados Normal/Selected/Highlighted (Amarillo)
                reino.botonesNiveles[i].interactable = puedeJugar;
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
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("¡Progreso de La Parca eliminado por completo!");

        ActualizarBotonesUI();
        EnfocarPrimerBoton();
    }
    /*
    [Header("Primer Botón")]
    [SerializeField] private Button botonNivel1;

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

    private System.Collections.IEnumerator Start()
    {
        // 1. Cargar el estado interactuable de los niveles
        ActualizarBotonesUI();


        // 2. Limpiar la selección previa
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // 3. ESPERAR UN FRAME: Esto le da tiempo a Unity para renderizar el Canvas 
        // y calcular las posiciones reales de los botones en la pantalla.
        yield return new WaitForEndOfFrame();

        // 4. Forzar la selección y colocar la guadaña
        if (botonNivel1 != null)
        {
            if (indicadorVisual != null)
            {
                indicadorVisual.gameObject.SetActive(true);
            }

            botonNivel1.Select();
            ultimoSeleccionado = botonNivel1.gameObject;

            // Ahora que esperamos el frame, el cálculo matemático será perfecto
            ActualizarPosicionIndicador(botonNivel1.gameObject);
        }
        else
        {
            EnfocarPrimerBoton();
        }

        ultimaPosicionMouse = Input.mousePosition;
    }

    private void Update()
    {
        // ESCUDO DE SEGURIDAD PRINCIPAL: Si no hay EventSystem o no hay ningún botón seleccionado, no hacemos nada
        if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject == null)
        {
            return;
        }

        if (indicadorVisual == null) return;

        // Detección de movimiento real del mouse para evitar interferencias falsas
        bool mouseSeMovio = Vector3.Distance(Input.mousePosition, ultimaPosicionMouse) > 0.1f;
        ultimaPosicionMouse = Input.mousePosition;

        GameObject seleccionadoActual = EventSystem.current.currentSelectedGameObject;

        // Si el mouse se movió y seleccionó algo por hover, le quitamos prioridad 
        if (mouseSeMovio && PointerOverUIObject(seleccionadoActual))
        {
            // Puedes descomentar la línea de abajo si quieres ignorar por completo el mouse:
            // return; 
        }

        // Mover las guadañas al botón enfocado actualmente de forma segura
        if (seleccionadoActual != ultimoSeleccionado)
        {
            ActualizarPosicionIndicador(seleccionadoActual);
        }
    }

    private void ActualizarPosicionIndicador(GameObject objetivo)
    {
        if (objetivo == null || indicadorVisual == null) return;

        RectTransform botonRect = objetivo.GetComponent<RectTransform>();

        // ESCUDO LÍNEA 90: Verificamos que tenga RectTransform y que tenga un padre asignado
        if (botonRect != null && botonRect.parent != null && indicadorVisual.parent != null)
        {
            ultimoSeleccionado = objetivo;

            Transform padreBoton = botonRect.parent;
            Transform padreIndicador = indicadorVisual.parent;

            // Cálculo matemático exacto relativo al lienzo principal
            Vector3 posicionFinal = padreIndicador.InverseTransformPoint(padreBoton.TransformPoint(botonRect.localPosition));
            indicadorVisual.localPosition = posicionFinal + offsetIndicador;
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

                // Tu nomenclatura dinámica perfecta: Nivel_1-1, Nivel_1-2...
                string clave = "Nivel_" + reino.numeroReino + "-" + (i + 1);
                int estadoDesbloqueado = PlayerPrefs.GetInt(clave, 0);

                bool puedeJugar = false;

                // El Reino 1 Nivel 1 siempre se puede jugar de forma nativa
                if (reino.numeroReino == 1 && i == 0)
                {
                    puedeJugar = true;
                }
                else
                {
                    puedeJugar = (estadoDesbloqueado == 1);
                }

                // Asignar estado lógico de interacción
                reino.botonesNiveles[i].interactable = puedeJugar;

                // --- FEEDBACK VISUAL LIMPIO ---
                // Forzamos un cambio de color directo en la imagen del botón para que el jugador
                // note inmediatamente qué nivel está disponible y cuál bloqueado.
                if (reino.botonesNiveles[i].image != null)
                {
                    // Si puede jugar: Color normal (Blanco puro). 
                    // Si está bloqueado: Oscuro y semitransparente.
                    reino.botonesNiveles[i].image.color = puedeJugar
                        ? Color.white
                        : new Color(0.2f, 0.2f, 0.2f, 0.6f);
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
                // ASEGURAR ENCIENDO EN LA FUNCIÓN DE RESPALDO:
                if (indicadorVisual != null)
                {
                    indicadorVisual.gameObject.SetActive(true);
                }

                reino.botonesNiveles[0].Select();
                ultimoSeleccionado = reino.botonesNiveles[0].gameObject;
                ActualizarPosicionIndicador(reino.botonesNiveles[0].gameObject);
                break;
            }
        }
    }

    public void ResetearTodoElProgreso()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("¡Progreso eliminado por completo!");

        ActualizarBotonesUI();
        EnfocarPrimerBoton();
    }
    */

}
