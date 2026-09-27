using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Play()
    {
        GameManager.Instance.LoadGame();
    }
}