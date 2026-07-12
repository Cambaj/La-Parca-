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

    private IEnumerator ChangeSceneAfterCountdown()
    {
        yield return new WaitForSeconds(countdownTillNextScene);

        int currentScene = SceneManager.GetActiveScene().buildIndex;
        int nextScene = currentScene + 1;

        if (nextScene < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextScene);
        }
        else
        {
            Debug.Log("No hay una siguiente escena en el Build Settings.");
        }
    }
}
