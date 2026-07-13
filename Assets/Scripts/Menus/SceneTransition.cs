using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private float countdownTillNextScene = 5f;

    private void Start()
    {
        StartCoroutine(ChangeSceneAfterCountdown());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.JoystickButton9))
        {
            LoadNextScene();
        }
    }

    private IEnumerator ChangeSceneAfterCountdown()
    {
        yield return new WaitForSeconds(countdownTillNextScene);
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;

        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("No hay una siguiente escena en el Build Settings. Se vuelve al menu (Escena 1)");
            SceneManager.LoadScene(1);
        }
    }
}