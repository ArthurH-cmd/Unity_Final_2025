using UnityEngine;
using UnityEngine.UI;

public class Quit_Game : MonoBehaviour
{
    public Button quitButton;

    private void Start()
    {
        Button quit = quitButton.GetComponent<Button>();
        quit.onClick.AddListener(QuitGame);
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
