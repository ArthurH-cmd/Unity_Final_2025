using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public Button starButton;
    public Button quitButton;

    void Start()
    {
        Button startButton = starButton.GetComponent<Button>();
        startButton.onClick.AddListener(TaskOnClick);

        Button quit = quitButton.GetComponent<Button>();
        quitButton.onClick.AddListener(QuitGame);
    } 
    void TaskOnClick()
    {
        SceneManager.LoadScene("THE_RING");
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
