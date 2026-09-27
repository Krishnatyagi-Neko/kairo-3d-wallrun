using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    public void Retry()
    {
        GameManager.Instance.LoadGame();
    }

    public void MainMenu()
    {
        GameManager.Instance.LoadMainMenu();
    }
}