using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToTheMainMenu : MonoBehaviour
{
    [SerializeField]
    private float delayBeforeMainMenu = 4f;

    private void Start()
    {
        StartCoroutine(LoadMainMenuAfterDelay());
    }

    private System.Collections.IEnumerator LoadMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeMainMenu);
        SceneManager.LoadScene("Main_Menu");
    }
}
