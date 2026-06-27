using UnityEngine;

public class OcultarPausaAlInicio : MonoBehaviour
{
    private void Start()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.pausePanel = this.gameObject;
        }
        gameObject.SetActive(false);
    }
}
