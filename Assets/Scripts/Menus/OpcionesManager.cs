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

    private void Start()
    {
        // 1. Ocultar por completo las guadañas al iniciar el menú
        if (indicadorVisual != null)
        {
            indicadorVisual.gameObject.SetActive(false);
        }

        // 2. Cargar trucos desde el LevelManager global si existe
        if (LevelManager.instance != null)
        {
            toggleInmortal.isOn = LevelManager.instance.cheatInmortal;
            toggleSaltarNiveles.isOn = LevelManager.instance.cheatSaltarEscenas;
        }

        toggleInmortal.onValueChanged.AddListener(SetCheatInmortal);
        toggleSaltarNiveles.onValueChanged.AddListener(SetCheatSaltarNiveles);
    }

    // ==========================================
    // MÉTODOS PÚBLICOS PARA EL EVENT TRIGGER
    // ==========================================

    // Se llamará cuando el mouse entre al botón Volver
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

    // Se llamará cuando el mouse salga del botón Volver
    public void OcultarGuadañas()
    {
        if (indicadorVisual != null)
        {
            indicadorVisual.gameObject.SetActive(false);
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
        Debug.Log("Cheat Saltar Niveles: " + valor);
    }
}
