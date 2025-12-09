using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private Script_Test player;
    [SerializeField]
    private Slider playerHealthSlider;

    private void Start()
    {
        if (player == null || playerHealthSlider == null)
        {
            Debug.LogError("HealthBar: missing references!");
            enabled = false;
            return;
        }

        playerHealthSlider.minValue = 0f;
        playerHealthSlider.maxValue = player.PlayerHealthMax;
    }

    private void Update()
    {
        playerHealthSlider.value = player.PlayerHealthCurrent;
    }
}
