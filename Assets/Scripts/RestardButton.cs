using UnityEngine;
using UnityEngine.UI;

public class RestartButton : MonoBehaviour
{
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnRestartClicked);
        }
    }

    private void OnRestartClicked()
    {
        if (LevelManager.I != null)
        {
            LevelManager.I.RestartGame();
        }
    }
}