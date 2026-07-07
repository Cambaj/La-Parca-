using UnityEngine;
using UnityEngine.UI;

public class CambiarEscenaBoton : MonoBehaviour
{
    [Tooltip("Escribe el nombre exacto de la escena a la que lleva este botón")]
    [SerializeField] private string escenaDestino;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null && LevelManager.instance != null)
        {
            btn.onClick.AddListener(() => LevelManager.instance.CargarEscena(escenaDestino));
        }
    }
}
