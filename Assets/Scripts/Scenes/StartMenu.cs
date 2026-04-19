using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void LoadFirstLevel()
    {
        SceneManager.LoadScene(1);
    }
}
