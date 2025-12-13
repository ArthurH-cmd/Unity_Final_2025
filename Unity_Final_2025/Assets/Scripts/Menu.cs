using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public Button starButton;

    void Start()
    {
        Button startButton = starButton.GetComponent<Button>();
        startButton.onClick.AddListener(TaskOnClick);
    } 
    void TaskOnClick()
    {
        SceneManager.LoadScene("THE_RING");
    }
}
